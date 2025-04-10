using System;
using System.Transactions;
using InventoryManagementSystem.Data;
using InventoryManagementSystem.Helpers;
using InventoryManagementSystem.Services.Interfaces;
using InventoryManagementSystem.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Services;

public class UserServices : IUserServices
{
    private readonly FirstRunDbContext dbContext;
    public UserServices(FirstRunDbContext dbContext){
        this.dbContext = dbContext;
    }
    //Create new User
    public async Task CreateUser(UserVm vm)
    {
        using var txn = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        bool emailExists= await dbContext.Users.AnyAsync( u => u.Email == vm.Email);
        //Check if the Email Alread exists or not 
        if(emailExists){
            throw new Exception("Email already Exists.");
        }
        PasswordValidator.PasswordStrengthValidator(vm.Password);

        var hashPassword = BCrypt.Net.BCrypt.HashPassword(vm.Password);
        var user = new Entity.User();
        user.UserName = vm.UserName;
        user.DOB = vm.DOB;
        user.Email = vm.Email;
        user.Address = vm.Address;
        user.Phone = vm.Phone;
        user.Role = vm.Role;
        user.Password = hashPassword;
        user.CreatedAt = DateTime.UtcNow;

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        txn.Complete();
    }

    //View User Detail
    public async Task<UserVm> GetUserById(int id){
        var user = await dbContext.Users.Where( u => u.UserId == id).Select(u=> new UserVm{
            UserId = u.UserId,
            UserName = u.UserName,
            Email = u.Email,
            DOB = u.DOB,
            Address = u.Address,
            Phone = u.Phone,
            Role = u.Role,
            CreatedAt = u.CreatedAt
        }).FirstOrDefaultAsync();
        if(user == null ){
            throw new UserNotFoundException("User not Found.");
        }
        return user;
    }
}