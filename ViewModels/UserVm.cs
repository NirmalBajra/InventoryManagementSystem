using System;
using InventoryManagementSystem.Constants;

namespace InventoryManagementSystem.ViewModels;

public class UserVm
{
    public int UserId { get; set;}
    public string UserName { get; set;}
    public DateTime DOB { get; set;}
    public string Email { get; set;}
    public string Address { get; set; }
    public string Phone { get; set;}
    public string UserStatus { get; set;} = UserStatusConstrants.Active;
    public string Role { get; set;}
    public string Password { get; set;}
    public DateTime CreatedAt { get; set;} = DateTime.UtcNow;
}
