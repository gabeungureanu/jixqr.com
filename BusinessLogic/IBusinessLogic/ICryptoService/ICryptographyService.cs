namespace BusinessLogic.IBusinessLogic.ICryptoService
{
    public interface ICryptographyService
    {
        //ENCRYPT THE STRING VALUE USING DEFAULT KEY
        String Encrypt(String strDecryptedString);
        //DECRYPT THE STRING VALUE USING DEFAULT KEY
        String Decrypt(String strEncryptedString);
        //ENCRYPT THE STRING VALUE BASED ON INPUT KEY
        String EncryptWithKey(String strDecryptedString, String strKey);
        //DECRYPT THE STRING VALUE BASED ON INPUT KEY
        String DecryptWithKey(String strEncryptedString, String strKey);
        //GENERATE SALT
        string GenerateSalt(int length);
        //GENERATE HASH
        string CreateHashCode(string password, string salt);
        //COMPARED HASH
        bool CompareHash(string userinputPassword, string base64Password, string salt);
    }
}
