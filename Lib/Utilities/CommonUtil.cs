using System.Reflection;
using System.Runtime.CompilerServices;

namespace Lib.Utilities
{
    public class CommonUtil
    {
        /// <summary>
        /// 使用者帳號前處理
        /// <para>數字員編：補足5碼</para>
        /// <para>實習帳號(A開頭)：還原6碼</para>
        /// </summary>
        public string PreProcessUserId(string userId)
        {
            userId = userId.NullableToStr();

            if (userId.IsNumeric())
                userId = userId.PadLeft(5, '0'); // 數字員編：補足5碼
            else if (userId.StartsWith("A") && userId.Length == 5)
                userId = userId.SubStr(0, 1) + "0" + userId.SubStr(1); // 實習帳號：還原6碼

            return userId;
        }

        /// <summary>
        /// 使用者帳號後處理
        /// <para>實習帳號(A開頭)：截為5碼</para>
        /// </summary>
        public string PostProcessUserId(string userId)
        {
            userId = userId.NullableToStr();

            if (userId.StartsWith("A") && userId.Length == 6)
                userId = userId.SubStr(0, 1) + userId.SubStr(2); // 實習帳號：截為5碼

            return userId;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string CurrentMethodName([CallerMemberName] string memberName = "") =>
            memberName;

        /// <summary>
        /// Get type from assembly
        /// </summary>
        /// <param name="typeName">e.g.
        /// <para>typeof(Controls.PdsRecdWindow).FullName</para>
        /// <para>typeof(PdsWpfApp.LoginWindow).FullName</para>
        /// </param>
        /// <param name="assemblyFile">e.g. Controls.dll</param>
        public Type? GetTypeFromAssembly(string typeName, string assemblyFile = "")
        {
            //string className = typeof(Lib.HostUtil).Name; // class name, no namespace
            //string ns = typeof(Lib.HostUtil).Namespace; // namespace, no class name
            //string nsAndClassName = typeof(Lib.HostUtil).FullName; // namespace and class name
            //string methodName = nameof(Lib.HostUtil.CheckServerAvalible);

            Type? targetType = null;

            try
            {
                // Dynamically loads an assembly.
                // 此段用於尋找相依專案，因主專案不會編譯成 PdsWpfApp.dll
                if (!assemblyFile.IsNullOrWhiteSpace())
                    targetType = Assembly.LoadFrom(assemblyFile).GetType(typeName);
            }
            catch (Exception) { }

            try
            {
                if (targetType == null)
                {
                    // Returns the assembly of the type by enumerating loaded assemblies in the app domain.
                    // 此段用於尋找主專案 PdsWpfApp
                    Assembly[] loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
                    targetType = loadedAssemblies.Select(assembly => assembly.GetType(typeName))
                        .FirstOrDefault(type => type != null);
                }
            }
            catch (Exception) { }

            return targetType;
        }

        /// <summary>
        /// Invoke method from assembly
        /// </summary>
        /// <param name="methodName">e.g. nameof(Lib.HostUtil.GetHostName)</param>
        /// <param name="typeName">e.g. typeof(Lib.HostUtil).FullName</param>
        /// <param name="assemblyFile">e.g. Lib.dll</param>
        /// <param name="parameters">method parameters. 
        /// <para>e.g. new object[] { "parameter1" }</para>
        /// </param>
        public object? InvokeMethodFromAssembly(string methodName, string typeName, string assemblyFile = "",
            object[]? parameters = null)
        {
            object? result = null;

            try
            {
                Type? targetType = GetTypeFromAssembly(typeName, assemblyFile);

                if (targetType != null)
                {
                    MethodInfo? mi = targetType.GetMethod(methodName,
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

                    if (mi != null && mi.IsStatic)
                        result = mi?.Invoke(null, parameters);
                    else
                        result = mi?.Invoke(Activator.CreateInstance(targetType), parameters);
                }
            }
            catch (Exception) { }

            return result;
        }

    }
}
