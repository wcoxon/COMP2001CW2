using System;
using System.Web;
using System.Text;
using System.Security.Cryptography;
using System.Data.SqlClient;
using System.Data;

namespace WebApplication1
{
    
    public class DataAccess
    {
        private string Connection;

        public DataAccess(string connection)
        {
            Connection = connection;
            
        }
        
        public byte[] hash(string arg)
        {
            return new Rfc2898DeriveBytes(arg, 32, 100).GetBytes(32);
        }

        public byte[] SHA256WithSalt(string password,byte[] salt)
        {
            //initialise byte array for concatenation of password(plaintext) + salt
            byte[] saltedPlainPass = new byte[password.Length + salt.Length];
            //convert string password into a byte array and copy it to the salted password byte array
            Encoding.UTF8.GetBytes(password).CopyTo(saltedPlainPass, 0);
            //copy salt byte array into the salted password array onto the end of the password bytes
            salt.CopyTo(saltedPlainPass, password.Length);
            //hash the salted plaintext password to create the hashed salted password
            byte[] hashedSaltedPass = SHA256.Create().ComputeHash(saltedPlainPass);
            return hashedSaltedPass;
        }
        public void changePassword()
        {
            //probably change salt aswell
            //just go into the table, we may want some kind of identifier for authentication other than username
            //query for where the username and password are in there, then generate salt and hash the plaintext arg also given, and then replace the shit in there with the new stuff
            //later we want a table of old passwords, among other reasons so people can't juke the system by going back and forth between the same passwords
        }
        public byte[] generateSalt(int bytes)
        {

            byte[] salt = new byte[bytes];
            RNGCryptoServiceProvider rand = new RNGCryptoServiceProvider();
            rand.GetNonZeroBytes(salt);

            return salt;
        }
        public bool compareByteArrays(byte[] array1, byte[] array2)
        {
            if (array1.Length == array2.Length)
            {
                for(int x = 0; x< array1.Length;x++){
                    if (array1[x] != array2[x])
                    {
                        return false;
                    }

                }
                return true;
            }
            return false;
            
        }
        public string RegisterUser(User user)
        {
            //function overload just generates 32 byte salt if salt parameter not assigned
            return RegisterUser(user, generateSalt(32));
        }
        public string RegisterUser(User user, byte[] salt)//, out string responseMessage)
        {
            //initialise byte array for concatenation of password(plaintext) + salt
            byte[] saltedPlainPass = new byte[user.password.Length + salt.Length];
            //convert string password into a byte array and copy it to the salted password byte array
            Encoding.UTF8.GetBytes(user.password).CopyTo(saltedPlainPass, 0);
            //copy salt byte array into the salted password array onto the end of the password bytes
            salt.CopyTo(saltedPlainPass, user.password.Length);
            //hash the salted plaintext password to create the hashed salted password
            byte[] hashedPass = SHA256.Create().ComputeHash(saltedPlainPass);


            SqlConnection cnn = new SqlConnection(Connection);
            SqlCommand command = new SqlCommand("EXEC RegisterUser @FirstName, @LastName, @Email, @HashedPass, @Salt, @Response out", cnn);

            command.Parameters.AddWithValue("@FirstName", user.firstName);
            command.Parameters.AddWithValue("@LastName", user.lastName);
            command.Parameters.AddWithValue("@Email", user.email);
            command.Parameters.AddWithValue("@HashedPass", hashedPass);
            command.Parameters.AddWithValue("@Salt", salt);
            
            command.Parameters.Add("@Response", System.Data.SqlDbType.VarChar, 64).Direction = System.Data.ParameterDirection.Output;
            
            object response;
            
            command.Connection.Open();
            command.ExecuteNonQuery();
            response = command.Parameters["@Response"].Value;
            
            command.Connection.Close();

            return response.ToString();
            
        }
        public bool Validate(User user)
        {

            SqlConnection cnn = new SqlConnection(Connection);
            //cnn = new SqlConnection(Connection);

            SqlCommand command = new SqlCommand("Validateuser",cnn);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add("@Email", SqlDbType.VarChar, 64);
            command.Parameters["@Email"].Value = user.email;

            command.Parameters.Add("@Password", SqlDbType.VarBinary, 64);
            command.Parameters["@Password"].Value = Encoding.UTF8.GetBytes(user.password);

            command.Parameters.Add("@Validated", System.Data.SqlDbType.Int);
            command.Parameters["@Validated"].Direction = System.Data.ParameterDirection.ReturnValue;

            cnn.Open();
            command.ExecuteNonQuery();
            
            bool val = (int)command.Parameters["@Validated"].Value==1;
            cnn.Close();

            return val;
            
        }

        public void Update( User user,int userID)
        {
            byte[] hashedPass = null;
            byte[] salt = null;
            if (user.password != null)
            {
                salt = generateSalt(32);
                byte[] saltedPlainPass = new byte[user.password.Length + salt.Length];
                //convert string password into a byte array and copy it to the salted password byte array
                Encoding.UTF8.GetBytes(user.password).CopyTo(saltedPlainPass, 0);
                //copy salt byte array into the salted password array onto the end of the password bytes
                salt.CopyTo(saltedPlainPass, user.password.Length);
                //hash the salted plaintext password to create the hashed salted password
                hashedPass = SHA256.Create().ComputeHash(saltedPlainPass);
            }


            SqlConnection cnn;
            cnn = new SqlConnection(Connection);
            SqlCommand command = new SqlCommand("EXEC UpdateUser @FirstName,@LastName,@Email,@Password,@Salt,@userID", cnn);
            command.Parameters.AddWithValue("@FirstName", user.firstName ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@LastName", user.lastName ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Email", user.email ?? (object)DBNull.Value);

            command.Parameters.Add("@Password", SqlDbType.Binary, 32);
            command.Parameters["@Password"].Value = hashedPass ?? (object)DBNull.Value;

            command.Parameters.Add("@Salt", SqlDbType.Binary, 32);
            command.Parameters["@Salt"].Value = salt ?? (object)DBNull.Value;

            command.Parameters.AddWithValue("@userID", userID);

            cnn.Open();
            command.ExecuteNonQuery();
            cnn.Close();
        }
        public void Delete(int userID)
        {
            SqlConnection cnn;
            cnn = new SqlConnection(Connection);
            SqlCommand command = new SqlCommand("EXEC DeleteUser @userID", cnn);
            command.Parameters.AddWithValue("@userID", userID);
            cnn.Open();
            command.ExecuteNonQuery();
            cnn.Close();
        }

    }
}
