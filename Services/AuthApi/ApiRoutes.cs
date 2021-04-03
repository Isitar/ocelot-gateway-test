namespace AuthApi
{
    public class ApiRoutes
    {
        private const string Root = "api";

        private const string Version = "v2";

        private const string Base = Root + "/" + Version;

        public static class Auth
        {
            private const string AuthBase = Base + "/auth";
            public const string Login = AuthBase + "/login";
            public const string Register = AuthBase + "/register";
            public const string LoginAz = AuthBase + "/login-azad";
            public const string AuthenticationType = AuthBase + "/auth-type";
            public const string Logout = AuthBase + "/logout";
            public const string LogoutEverywhere = AuthBase + "/logout-everywhere";
            public const string Refresh = AuthBase + "/refresh";
            public const string ResetPassword = AuthBase + "/reset-password";

            public const string AddRole = AuthBase + "/{id}/role";
            public const string RemoveRole = AuthBase + "/{id}/role/{roleName}";
            public const string UserRoles = AuthBase + "/{id}/role";

            public const string AddPermission = AuthBase + "/{id}/permission";
            public const string RemovePermission = AuthBase + "/{id}/permission/{permissionName}";
            public const string UserPermissions = AuthBase + "/{id}/permission";
        }
    }
}