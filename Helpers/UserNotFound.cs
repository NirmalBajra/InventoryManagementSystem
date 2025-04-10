using System;

namespace InventoryManagementSystem.Helpers;

public class UserNotFoundException : Exception
{
    public UserNotFoundException(string message):base (message){ }
}
