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
        //private string username;
        //private byte[] passwordHash;
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
        public string RegisterUser(User user)//, out string responseMessage)
        {
            //function overload just generates 32 byte salt if salt parameter not assigned
            return RegisterUser(user, generateSalt(32));//, out responseMessage);
        }
        public string RegisterUser(User user, byte[] salt)//, out string responseMessage)
        {
            //initialise byte array for concatenation of password(plaintext) + salt
            byte[] saltedPlainPass = new byte[user.Password.Length + salt.Length];
            //convert string password into a byte array and copy it to the salted password byte array
            Encoding.UTF8.GetBytes(user.Password).CopyTo(saltedPlainPass, 0);
            //copy salt byte array into the salted password array onto the end of the password bytes
            salt.CopyTo(saltedPlainPass, user.Password.Length);
            //hash the salted plaintext password to create the hashed salted password
            byte[] hashedPass = SHA256.Create().ComputeHash(saltedPlainPass);


            SqlConnection cnn = new SqlConnection(Connection);// "Data Source=localhost\\MSSQLSERVER04;Initial Catalog=StockDB;Integrated Security=True");
                                                              //SqlCommand command = new SqlCommand("INSERT INTO Users VALUES (@FirstName,@LastName, @HashedSaltedPass, @Salt);", cnn);

            //SqlParameter responseParam = new SqlParameter("@ResponseMessage", System.Data.SqlDbType.VarChar, 64);
            //responseParam.Direction = System.Data.ParameterDirection.Output;
            SqlCommand command = new SqlCommand("EXEC RegisterUser @FirstName, @LastName, @Email, @HashedPass, @Salt, @Response out", cnn);

            //Console.WriteLine(usernameParam);
            //string response = "";
            //string responseText = "";
            command.Parameters.AddWithValue("@FirstName", user.FirstName);
            command.Parameters.AddWithValue("@LastName", user.LastName);
            command.Parameters.AddWithValue("@Email", user.Email);
            command.Parameters.AddWithValue("@HashedPass", hashedPass);
            command.Parameters.AddWithValue("@Salt", salt);
            //command.Parameters.Add("@ResponseMessage", System.Data.SqlDbType.VarChar, 64).Direction = System.Data.ParameterDirection.Output;
            //command.Parameters.AddWithValue("@Response", response);
            //command.Parameters["@ResponseMessage"].Direction = System.Data.ParameterDirection.Output;
            command.Parameters.Add("@Response", System.Data.SqlDbType.VarChar, 64).Direction = System.Data.ParameterDirection.Output;
            
            //command.Parameters["@Response"].Direction = System.Data.ParameterDirection.Output;
            //command.Parameters["@Response"].Direction = System.Data.ParameterDirection.Output;
            //SqlDataReader sdr = command.ExecuteReader();
            object response;
            //command.Parameters["@ResponseMessage"].Value = responseText;
            //response = command.Parameters["@ResponseMessage"].Value = "";
            //command.Parameters["@Response"].Value="a";
            //command.par

            //command.Parameters.Add(responseParam);

            //response = command.Parameters["@ResponseMessage"];
            command.Connection.Open();
            command.ExecuteNonQuery();
            response = command.Parameters["@Response"].Value;
            //response = responseParam.Value;
            command.Connection.Close();
            return response.ToString();
            //return command.Parameters["@Reponse"].Value.ToString();
        }
        public bool Validate(User user)
        {

            SqlConnection cnn;
            cnn = new SqlConnection(Connection);// "Data Source=localhost\\MSSQLSERVER04;Initial Catalog=StockDB;Integrated Security=True");

            //SqlCommand command = new SqlCommand("SELECT Salt,HashP FROM Users WHERE Username = @UsernameParam", cnn);
            //SqlCommand command = new SqlCommand("SELECT Username FROM Users WHERE Username = @UsernameParam AND Salt", cnn);
            SqlCommand command = new SqlCommand("EXEC ValidateUser @Email,@Password", cnn);
            //Console.WriteLine(usernameParam);
            command.Parameters.AddWithValue("@Email", user.Email);
            command.Parameters.AddWithValue("@Password", user.Password);
            command.Parameters.Add("@Validated", System.Data.SqlDbType.Int).Direction = System.Data.ParameterDirection.ReturnValue;

            command.Connection.Open();
            command.ExecuteNonQuery();
            command.Connection.Close();
            return (int)command.Parameters["@Validated"].Value==1;
            //SqlDataReader sdr = command.ExecuteReader();
            //command.Connection.Close();
            //sdr.Close();
            //command.Connection.Close();
            //Console.WriteLine(sdr.Read());
            //Console.WriteLine(sd)
            //byte[] saltbuffer = new byte[32];
            //byte[] bytebuffer2 = new byte[32];
            //Console.WriteLine("hey");
            /*while (sdr.Read())
            {
                //sdr.Read();
                sdr.GetBytes(0, 0, saltbuffer, 0, 32);
                sdr.GetBytes(1, 0, bytebuffer2, 0, 32);
                
                if (compareByteArrays(SHA256WithSalt(user.Password, saltbuffer),bytebuffer2))
                {
                    sdr.Close();
                    command.Connection.Close();
                    return 1;//sdr.GetInt32(0).ToString();
                }
                //return sdr.GetInt32(0).ToString();
            }

            //sdr.Read
            //command.Connection.Close();
            sdr.Close();
            command.Connection.Close();
            return 0;*/
            //"SELECT Salt FROM USERS WHERE Username = PARAM"
            //setPassword(passwordParam);
            /*if(username == usernameParam)
            {
                if (passwordHash == hash(passwordParam))
                {
                    log_in();
                    //return 1;
                }
                
            }*/
            //return 0;
        }

        public void Update( User user,int userID)
        {
            byte[] hashedPass = null;
            byte[] salt = null;
            if (user.Password != null)
            {
                salt = generateSalt(32);
                byte[] saltedPlainPass = new byte[user.Password.Length + salt.Length];
                //convert string password into a byte array and copy it to the salted password byte array
                Encoding.UTF8.GetBytes(user.Password).CopyTo(saltedPlainPass, 0);
                //copy salt byte array into the salted password array onto the end of the password bytes
                salt.CopyTo(saltedPlainPass, user.Password.Length);
                //hash the salted plaintext password to create the hashed salted password
                hashedPass = SHA256.Create().ComputeHash(saltedPlainPass);
            }


            SqlConnection cnn;
            cnn = new SqlConnection(Connection);
            SqlCommand command = new SqlCommand("EXEC UpdateUser @FirstName,@LastName,@Email,@Password,@Salt,@userID", cnn);
            command.Parameters.AddWithValue("@FirstName", user.FirstName ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@LastName", user.LastName ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Email", user.Email ?? (object)DBNull.Value);

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
