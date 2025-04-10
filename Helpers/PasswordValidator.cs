using System;

namespace InventoryManagementSystem.Helpers;

public class PasswordValidator
{
    public static bool PasswordStrengthValidator(string password)
    {
        if(password.Length < 6){
            throw new Exception("Password length must be at least 6 character long.");
        }
        if(!password.Any(char.IsLower)){
            throw new Exception("Password must contain at least one lowercase letter.");
        }
        if(!password.Any(char.IsUpper)){
            throw new Exception("Password must contain at least one uppercase letter.");
        }
        if(!password.Any(c => !char.IsLetterOrDigit(c))){
            throw new Exception("Password must contain at least one specail character.");
        }
        return true;
    }
}
