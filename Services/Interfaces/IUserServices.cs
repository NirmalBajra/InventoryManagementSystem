using System;
using InventoryManagementSystem.ViewModels;

namespace InventoryManagementSystem.Services.Interfaces;

public interface IUserServices
{
    Task CreateUser(UserVm vm);
    Task<UserVm> GetUserById(int userId);
    // Task<UserVm> DeleteUser(int userId);
    // Task<UserVm> UpdateUser(int userId, UserVm vm);
}   
