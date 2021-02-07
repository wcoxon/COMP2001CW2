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
        private string ConnectionString;

        public DataAccess(string connectionString)
        {
            ConnectionString = connectionString;
        }
        
        public byte[] generateSalt(int bytes)
        {
            //generates a random salt value. bytes parameter represents the length of the salt being generated
            byte[] salt = new byte[bytes];
            RNGCryptoServiceProvider rand = new RNGCryptoServiceProvider();
            rand.GetNonZeroBytes(salt);

            return salt;
        }
        
        public byte[] SaltAndHash(string password,byte[] salt)
        {
            //this function creates a byte array buffer, which it uses to concatenate the password string's bytes and the salt
            //once the password is salted, the bytes are hashed using SHA256, and the hash is returned
            byte[] saltedPassword = new byte[password.Length + salt.Length];
            Encoding.UTF8.GetBytes(password).CopyTo(saltedPassword, 0);
            salt.CopyTo(saltedPassword, password.Length);

            return SHA256.Create().ComputeHash(saltedPassword);
        }

        public string Register(User user)
        {
            string responseMessage;

            byte[] salt = generateSalt(32);
            byte[] hashedPass = SaltAndHash(user.password, salt);

            //create the connection and command. the command executes a stored procedure.
            SqlConnection connection = new SqlConnection(ConnectionString);
            SqlCommand command = new SqlCommand("RegisterUser", connection);
            command.CommandType = CommandType.StoredProcedure;

            //add the parameters to the command
            command.Parameters.Add("@FirstName",SqlDbType.VarChar,32);
            command.Parameters.Add("@LastName", SqlDbType.VarChar, 32);
            command.Parameters.Add("@Email", SqlDbType.VarChar, 64);
            command.Parameters.Add("@Password", SqlDbType.Binary, 32);
            command.Parameters.Add("@Salt", SqlDbType.Binary, 32);
            command.Parameters.Add("@ResponseMessage", System.Data.SqlDbType.VarChar, 64);

            //assign values to the command's parameters
            command.Parameters["@FirstName"].Value = user.firstName;
            command.Parameters["@LastName"].Value = user.lastName;
            command.Parameters["@Email"].Value = user.email;
            command.Parameters["@Password"].Value = hashedPass;
            command.Parameters["@Salt"].Value = salt;
            command.Parameters["@ResponseMessage"].Direction = System.Data.ParameterDirection.Output;

            connection.Open();
            command.ExecuteNonQuery();
            //set responseMessage to the string value of the output parameter in the RegisterUser procedure.
            responseMessage = command.Parameters["@ResponseMessage"].Value.ToString();

            connection.Close();

            //returns the response message after obtaining the value from the database
            return responseMessage;
        }

        public bool Validate(User user)
        {
            bool Validated;

            //create the connection and command. the command executes a stored procedure.
            SqlConnection connection = new SqlConnection(ConnectionString);
            SqlCommand command = new SqlCommand("ValidateUser",connection);
            command.CommandType = CommandType.StoredProcedure;

            //add the parameters to the command
            command.Parameters.Add("@Email", SqlDbType.VarChar, 64);
            command.Parameters.Add("@Password", SqlDbType.VarBinary, 64);
            command.Parameters.Add("@Validated", System.Data.SqlDbType.Int);

            //assign values to the command's parameters
            command.Parameters["@Email"].Value = user.email;
            command.Parameters["@Password"].Value = Encoding.UTF8.GetBytes(user.password);
            command.Parameters["@Validated"].Direction = System.Data.ParameterDirection.ReturnValue;

            connection.Open();
            command.ExecuteNonQuery();
            //get the return value in the form of a boolean, when the return is 1, return==1 is true
            //this result determines whether the user credentials are valid or not.
            Validated = (int)command.Parameters["@Validated"].Value==1;

            connection.Close();

            //returns the validation status after obtaining the value from the database
            return Validated;
        }

        public void Update( User user,int userID)
        {
            byte[] salt = null;
            byte[] passwordBytes = null;

            if (user.password != null)
            {
                salt = generateSalt(32);
                passwordBytes = Encoding.UTF8.GetBytes(user.password);
            }

            //create the connection and command. the command executes a stored procedure.
            SqlConnection connection = new SqlConnection(ConnectionString);
            SqlCommand command = new SqlCommand("UpdateUser", connection);
            command.CommandType = CommandType.StoredProcedure;

            //add the parameters to the command
            command.Parameters.Add("@FirstName", SqlDbType.VarChar,32);
            command.Parameters.Add("@LastName", SqlDbType.VarChar, 32);
            command.Parameters.Add("@Email", SqlDbType.VarChar, 64);
            command.Parameters.Add("@Password", SqlDbType.VarBinary, 64);
            command.Parameters.Add("@Salt", SqlDbType.Binary, 32);
            command.Parameters.Add("@id", SqlDbType.Int);

            //assign values to the command's parameters
            command.Parameters["@FirstName"].Value = user.firstName ?? (object)DBNull.Value;
            command.Parameters["@LastName"].Value = user.lastName ?? (object)DBNull.Value;
            command.Parameters["@Email"].Value = user.email ?? (object)DBNull.Value;
            command.Parameters["@Password"].Value = passwordBytes ?? (object)DBNull.Value;
            command.Parameters["@Salt"].Value = salt ?? (object)DBNull.Value;
            command.Parameters["@id"].Value = userID;

            //open the connection momentarily to execute the stored procedure
            connection.Open();
            command.ExecuteNonQuery();
            connection.Close();
        }

        public void Delete(int userID)
        {
            //create the connection and command. the command executes a stored procedure.
            SqlConnection connection = new SqlConnection(ConnectionString);
            SqlCommand command = new SqlCommand("DeleteUser", connection);
            command.CommandType = CommandType.StoredProcedure;

            //add the parameter to the command
            command.Parameters.Add("@id", SqlDbType.Int);

            //assign value to the command's parameter
            command.Parameters["@id"].Value = userID;

            //open the connection momentarily to execute the stored procedure
            connection.Open();
            command.ExecuteNonQuery();
            connection.Close();
        }

    }
}
