using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;

// to talk to mysql
using MySql.Data;
using MySql.Data.MySqlClient;

// to manipulate data from a db
using System.Data;
using System.Net;
using System.Net.Mail;

namespace gamingProject
{
    /// <summary>
    /// Summary description for WebService1
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class WebService1 : System.Web.Services.WebService
    {

        // creating an account with email & password
        [WebMethod]
        public string CreateAccount(string username, string email, string password, string re_enter)
        {
            // if password confirmation matches & email is never used, the account will be created
            if (password == re_enter)
            {
                string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
                string sqlSelect = "INSERT INTO `pj2`.`users` (`username`, `email`, `password`) VALUES (@usernameValue, @emailValue, @passwordValue);";

                MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
                MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

                sqlCommand.Parameters.AddWithValue("@usernameValue", HttpUtility.UrlDecode(email));
                sqlCommand.Parameters.AddWithValue("@emailValue", HttpUtility.UrlDecode(email));
                sqlCommand.Parameters.AddWithValue("@passwordValue", HttpUtility.UrlDecode(password));

                sqlConnection.Open();
                try
                {
                    sqlCommand.ExecuteNonQuery();
                    return "Account Created.";
                }

                catch (Exception e)
                {
                    var e_str = e.ToString();

                    if (e_str.Contains("email_UNIQUE"))
                    {
                        return "The email addresss you entered is already in use.";
                    }

                    else
                    {
                        return e_str;
                    }
                }
                sqlConnection.Close();
            }

            else
            {
                return "The password confirmation does not match.";
            }
        }

    }
}
