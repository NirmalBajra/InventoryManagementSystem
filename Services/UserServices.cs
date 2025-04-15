using System;
using System.Security.Claims;
using System.Transactions;
using InventoryManagementSystem.Data;
using InventoryManagementSystem.Dto;
using InventoryManagementSystem.Helpers;
using InventoryManagementSystem.Services.Interfaces;
using InventoryManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementSystem.Services;

public class UserServices : IUserServices
{
    private readonly FirstRunDbContext dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    public UserServices(FirstRunDbContext dbContext,IHttpContextAccessor httpContextAccessor){
        this.dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
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
            UserStatus = u.UserStatus,
            Role = u.Role,
            CreatedAt = u.CreatedAt
        }).FirstOrDefaultAsync();
        if(user == null ){
            throw new UserNotFoundException("User not Found.");
        }
        return user;
    }
    public async Task Login(LoginUserDto dto){
        using var txn = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if(user == null){
            throw new UserNotFoundException("Invalid Email.");
        }
        bool verified = BCrypt.Net.BCrypt.Verify(dto.Password,user.Password);
        if(!verified){
            throw new Exception("Invalid Password");
        }

        var httpContext = _httpContextAccessor.HttpContext;
        var claims = new List<Claim>
        {
            new("Id",user.UserId.ToString()),
            new("Username",user.UserName),
            new("Role",user.Role)
        };
        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
        txn.Complete();
    }

    //Logout 
    public async Task Logout(){
        await _httpContextAccessor.HttpContext.SignOutAsync();
    }
}