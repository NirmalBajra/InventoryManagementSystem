using System;
using System.ComponentModel.DataAnnotations;
using InventoryManagementSystem.Constants;
using InventoryManagementSystem.Controllers;

namespace InventoryManagementSystem.Entity;

public class User
{
    [Key]
    public int UserId { get; set;}
    public string UserName { get; set;}
    public DateTime DOB { get; set;}
    public string Email { get; set;}
    public string Address { get; set; }
    public string Phone { get; set;}
    public string UserStatus { get; set;} = UserStatusConstrants.Active;
    public string Role { get; set;} = UserTypeConstrants.Normaluser;
    public string Password { get; set;}
    public DateTime CreatedAt { get; set;} = DateTime.UtcNow;
}
