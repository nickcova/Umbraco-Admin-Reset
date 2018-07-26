using System;

namespace Umbraco_6.X_Admin_Reset.Exceptions
{
    public class MembershipProviderMissingException : Exception
    {
        public MembershipProviderMissingException()
        {
        }

        public MembershipProviderMissingException(string message)
        : base(message)
        {
        }

        public MembershipProviderMissingException(string message, Exception inner)
        : base(message, inner)
        {
        }
    }
}