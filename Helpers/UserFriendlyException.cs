using System;

namespace InventoryManagementSystem.Helpers;

public class UserFriendlyException : Exception
{
    public UserFriendlyException(string message) : base(message){ }
}
