using BusinessLogic.IBusinessLogic.ICryptoService;
using System.Security.Cryptography;
using System.Text;

namespace BusinessLogic.BLImplementation.CryptoService
{
    public class CryptographyService : ICryptographyService
    {
        #region Cryptography Functions        
        //private const String DEFAULTKEY = "Jubilee@AB9BB97A-E99C-4279-B944-784A6B04E69F";//DELEULT ENCRYPTED/DECRYPTED KEY
        private const String DEFAULTKEY = "QUAAPPS@2020";

        private String mstrErrorString = String.Empty;
        private String mstrOutputString = String.Empty;

        //CODE TO GET THE MD5 HASH VALUE FOR A STRING
        static Byte[] GetMD5Hash(String strKey)
        {
            MD5CryptoServiceProvider objHashMD5 = null;
            Byte[] objPwdhash;
            try
            {
                objHashMD5 = new MD5CryptoServiceProvider();
                objPwdhash = objHashMD5.ComputeHash(ASCIIEncoding.ASCII.GetBytes(strKey));

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (objHashMD5 != null)
                {
                    objHashMD5.Clear();
                }
            }
            return objPwdhash;
            //RETUNRING THE BYTE ARRAY FOR THE INPUT STRING
        }

        //CODE TO ENCRYPT THE STRING BASED ON THE DEFAULT KEY
        public String Encrypt(String strDecryptedString)
        {
            try
            {
                return EncryptWithKey(strDecryptedString, DEFAULTKEY).Replace("+", "^^").Replace("=", "~~");
                //CODE TO RETURN THE ENCRYPT STRING
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        //CODE FOR DECRYPT THE ENCRYPTED STRING
        public String Decrypt(String strEncryptedString)
        {
            try
            {
                return DecryptWithKey(strEncryptedString.Replace("^^", "+").Replace("~~", "="), DEFAULTKEY);
                //CODE FOR RETURNING THE DECRYPT STRING BASED ON DEFAULT KEY
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        //CODE FOR ENCRYPT THE STRING
        public String EncryptWithKey(String strDecryptedString, String strKey)
        {
            String strEncrypted = String.Empty;
            TripleDESCryptoServiceProvider objDES = null;
            Byte[] objBuff;
            try
            {
                objDES = new TripleDESCryptoServiceProvider();
                if (strKey.Length > 0)
                {
                    objDES.Key = GetMD5Hash(strKey);
                }
                objDES.Mode = CipherMode.ECB;
                objBuff = ASCIIEncoding.ASCII.GetBytes(strDecryptedString);
                strEncrypted = Convert.ToBase64String(objDES.CreateEncryptor().TransformFinalBlock(objBuff, 0, objBuff.Length));
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (objDES != null)
                {
                    objDES.Clear();
                }
            }
            return strEncrypted;
            //CODE TO RETURN THE ENCRYPT STRING BASED ON THEIR INPUT KEY VALUE
        }
        //CODE FOR DECRYPT THE STRING
        public String DecryptWithKey(String strEncryptedString, String strKey)
        {
            String strDecrypted = String.Empty;
            TripleDESCryptoServiceProvider objDES = null;
            Byte[] objBuff;
            try
            {
                if (strKey.Length == 0)
                {
                    throw new Exception("Invalid key supplied. Unable to decrypt.");
                }
                objDES = new TripleDESCryptoServiceProvider();
                if (strKey.Length > 0)
                {
                    objDES.Key = GetMD5Hash(strKey);
                }
                objDES.Mode = CipherMode.ECB;
                objBuff = Convert.FromBase64String(strEncryptedString);
                strDecrypted = ASCIIEncoding.ASCII.GetString(objDES.CreateDecryptor().TransformFinalBlock(objBuff, 0, objBuff.Length));
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (objDES != null)
                {
                    objDES.Clear();
                }
            }
            return strDecrypted;
            //CODE TO RETURN THE DECRYPT THE STRING BASED ON THEIR INPUT KEY VALUE
        }

        #region SALT
        //GENERATE SALT
        public string GenerateSalt(int length)
        {
            var random = new RNGCryptoServiceProvider();
            // Maximum length of salt
            int max_length = length;
            // Empty salt array
            byte[] salt = new byte[max_length];
            // Build the random bytes
            random.GetNonZeroBytes(salt);
            // Return the string encoded salt
            return Convert.ToBase64String(salt);
        }
        //GENERATE HASH
        public string CreateHashCode(string password, string salt)
        {
            try
            {
                byte[] byteArray = ASCIIEncoding.ASCII.GetBytes(string.Concat(salt, password));
                byte[] hashbyteArray = new MD5CryptoServiceProvider().ComputeHash(byteArray);
                return ByteArrayToString(hashbyteArray);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        //BYTE TO STRING
        public string ByteArrayToString(byte[] arrInput)
        {
            try
            {
                int i;
                StringBuilder sOutput = new StringBuilder(arrInput.Length);
                for (i = 0; i < arrInput.Length - 1; i++)
                {
                    sOutput.Append(arrInput[i].ToString("X2"));
                }
                return sOutput.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        //COMPARED HASED STRING
        public bool CompareHash(string userinputPassword, string base64Password, string salt)
        {
            string hashPassword = CreateHashCode(userinputPassword, salt);
            return hashPassword == base64Password;
        }
        #endregion SALT

        #endregion encryption password
    }
}
