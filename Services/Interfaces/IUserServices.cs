using System;
using InventoryManagementSystem.Dto;
using InventoryManagementSystem.ViewModels;

namespace InventoryManagementSystem.Services.Interfaces;

public interface IUserServices
{
    Task CreateUser(UserVm vm);
    Task<UserVm> GetUserById(int userId);
    Task Login(LoginUserDto dto);
    Task Logout();
    // Task<UserVm> DeleteUser(int userId);
    // Task<UserVm> UpdateUser(int userId, UserVm vm);
}   
