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
    [System.Web.Script.Services.ScriptService]
    public class WebService1 : System.Web.Services.WebService
    {
        // creating an account with email & password
        [WebMethod]
        public string CreateAccount(string username, string email, string password, string re_enter)
        {
            // if password confirmation matches & email &/ username is never used, the account will be created
            if (password == re_enter)
            {
                string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
                string sqlSelect = "INSERT INTO `pj2`.`users` (`username`, `email`, `password`) VALUES (@usernameValue, @emailValue, @passwordValue);";

                MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
                MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

                sqlCommand.Parameters.AddWithValue("@usernameValue", HttpUtility.UrlDecode(username));
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

                    if (e_str.Contains("username_UNIQUE"))
                    {
                        return "The username you entered is already in use.";
                    }
                    else if (e_str.Contains("email_UNIQUE"))
                    {
                        return "The email address you entered is already in use.";
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
                return "The passwords do not match.";
            }
        }

        // able to log in to the account
        [WebMethod(EnableSession = true)]
        public bool Login(string username, string password)
        {
            bool success = false;

            string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
            string sqlSelect = "SELECT * FROM pj2.users WHERE username = @usernameValue AND password = @passwordValue";

            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            sqlCommand.Parameters.AddWithValue("@usernameValue", HttpUtility.UrlDecode(username));
            sqlCommand.Parameters.AddWithValue("@passwordValue", HttpUtility.UrlDecode(password));

            MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
            DataTable sqlDt = new DataTable("accounts");
            sqlDa.Fill(sqlDt);

            if (sqlDt.Rows.Count > 0)
            {
                // create session which allows to retrieve data among a particular user
                Session["user_id"] = sqlDt.Rows[0]["user_id"];
                Session["username"] = sqlDt.Rows[0]["username"];
                Session["email"] = sqlDt.Rows[0]["email"];
                Session["password"] = sqlDt.Rows[0]["password"];
                Session["game_played"] = sqlDt.Rows[0]["game_played"];
                Session["rank_tier"] = sqlDt.Rows[0]["rank_tier"];
                Session["rank_division"] = sqlDt.Rows[0]["rank_division"];

                success = true;
            }
            return success;
        }

        // able to log out to the account
        [WebMethod(EnableSession = true)]
        public bool Logout()
        {
            bool success = false;
            Session.Abandon();

            return success;
        }

        // list the info of the user who has already logged in 
        [WebMethod(EnableSession = true)]
        public Users[] ViewUser()
        {
            var username = Session["username"];

            DataTable sqlDt = new DataTable("Users");

            string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
            string sqlSelect = "SELECT * FROM pj2.users WHERE username = '" + username + "';";

            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
            sqlDa.Fill(sqlDt);

            List<Users> user = new List<Users>();
            for (int i = 0; i < sqlDt.Rows.Count; i++)
            {
                user.Add(new Users
                {
                    user_id = Convert.ToInt32(sqlDt.Rows[i]["user_id"]),
                    username = sqlDt.Rows[i]["username"].ToString(),
                    email = sqlDt.Rows[i]["email"].ToString(),
                    password = sqlDt.Rows[i]["password"].ToString(),
                });
            }
            return user.ToArray();
        }

        // save the gamename_id the users select to db
        [WebMethod(EnableSession = true)]
        public string SelectGame(int gameNum)
        {
            var user_id = Convert.ToInt32(Session["user_id"]);

            string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
            string sqlSelect = "UPDATE `pj2`.`users` SET `game_played` = (@gamePlayedValue) WHERE(`user_id` = " + user_id + ");";

            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            sqlCommand.Parameters.AddWithValue("@gamePlayedValue", Convert.ToInt32(gameNum));

            sqlConnection.Open();

            try
            {
                sqlCommand.ExecuteNonQuery();
                return "Added game_Played = " + gameNum + " into db";
            }

            catch (Exception e)
            {
                var e_str = e.ToString();
                return e_str;
            }
            sqlConnection.Close();
        }

        // allows users to select their rank tier and save it into the db [LOL & APEX] 
        [WebMethod(EnableSession = true)]
        public string SubmitRankInfo(string tier, string division, string role)
        {
            var user_id = Convert.ToInt32(Session["user_id"]);

            string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
            string sqlSelect = "UPDATE `pj2`.`users` SET `rank_tier` = (@rankTierValue), `rank_division` = (@rankDivisionValue), `role` = (@roleValue) WHERE(`user_id` = " + user_id + ");";

            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            sqlCommand.Parameters.AddWithValue("@rankTierValue", HttpUtility.UrlDecode(tier));
            sqlCommand.Parameters.AddWithValue("@rankDivisionValue", HttpUtility.UrlDecode(division));
            sqlCommand.Parameters.AddWithValue("@roleValue", HttpUtility.UrlDecode(role));

            sqlConnection.Open();

            try
            {        
                sqlCommand.ExecuteNonQuery();
                return "Tier, division, and role are saved";

            }

            catch (Exception e)
            {
                var e_str = e.ToString();
                return e_str;
            }
            sqlConnection.Close();
        }

        // allows users to select their rank tier and save it into the db [CSGO] 
        [WebMethod(EnableSession = true)]
        public string SubmitRank(string rank)
        {
            var user_id = Convert.ToInt32(Session["user_id"]);

            string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
            string sqlSelect = "UPDATE `pj2`.`users` SET `rank_tier` = (@rankTierValue), `rank_division` = NULL, `role` = NULL WHERE(`user_id` = " + user_id + ");";

            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            sqlCommand.Parameters.AddWithValue("@rankTierValue", HttpUtility.UrlDecode(rank));

            sqlConnection.Open();

            try
            {
                sqlCommand.ExecuteNonQuery();
                return "Rank is saved";

            }

            catch (Exception e)
            {
                var e_str = e.ToString();
                return e_str;
            }
            sqlConnection.Close();
        }

        // check whether the user has selected a game or not 
        [WebMethod(EnableSession = true)]
        public Users[] CheckNullGameUsers()
        {
            var username = Session["username"];

            DataTable sqlDt = new DataTable("Users");

            string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
            string sqlSelect = "SELECT * FROM pj2.users WHERE game_played is NULL AND username = '" + username + "';";

            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
            sqlDa.Fill(sqlDt);

            List<Users> user = new List<Users>();
            for (int i = 0; i < sqlDt.Rows.Count; i++)
            {
                user.Add(new Users
                {
                    user_id = Convert.ToInt32(sqlDt.Rows[i]["user_id"]),
                    username = sqlDt.Rows[i]["username"].ToString(),
                    email = sqlDt.Rows[i]["email"].ToString(),
                    password = sqlDt.Rows[i]["password"].ToString(),
                });
            }
            return user.ToArray();
        }




        // list all users info
        //[WebMethod]
        //public Users[] ViewUsers()
        //{
        //    DataTable sqlDt = new DataTable("Users");

        //    string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
        //    string sqlSelect = "SELECT * FROM pj2.users";

        //    MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
        //    MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

        //    MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
        //    sqlDa.Fill(sqlDt);

        //    List<Users> user = new List<Users>();
        //    for (int i = 0; i < sqlDt.Rows.Count; i++)
        //    {
        //        user.Add(new Users
        //        {
        //            user_id = Convert.ToInt32(sqlDt.Rows[i]["user_id"]),
        //            username = sqlDt.Rows[i]["username"].ToString(),
        //            email = sqlDt.Rows[i]["email"].ToString(),
        //            password = sqlDt.Rows[i]["password"].ToString(),
        //        });
        //    }
        //    return user.ToArray();
        //}

        //// list the gamename_id & game_name the user selects
        //[WebMethod]
        //public Games[] SelectGame(string gameName)
        //{
        //    DataTable sqlDt = new DataTable("Games");

        //    string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
        //    string sqlSelect = "SELECT * FROM pj2.gamesname WHERE game_name = @gameNameValue";

        //    MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
        //    MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

        //    sqlCommand.Parameters.AddWithValue("@gameNameValue", HttpUtility.UrlDecode(gameName));

        //    MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
        //    sqlDa.Fill(sqlDt);

        //    List<Games> game = new List<Games>();
        //    for (int i = 0; i < sqlDt.Rows.Count; i++)
        //    {
        //        game.Add(new Games
        //        {
        //            name_id = Convert.ToInt32(sqlDt.Rows[i]["name_id"]),
        //            game_name = sqlDt.Rows[i]["game_name"].ToString(),
        //        });
        //    }
        //    return game.ToArray();
        //}
    }
}
