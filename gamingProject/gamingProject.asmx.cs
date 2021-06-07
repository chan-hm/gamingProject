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
            var game_played = 0;

            // if password confirmation matches & email &/ username is never used, the account will be created
            if (password == re_enter)
            {
                string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
                string sqlSelect = "INSERT INTO `pj2`.`users` (`username`, `email`, `password`, `game_played`) VALUES (@usernameValue, @emailValue, @passwordValue, @gamePlayedValue);";

                MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
                MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

                sqlCommand.Parameters.AddWithValue("@usernameValue", HttpUtility.UrlDecode(username));
                sqlCommand.Parameters.AddWithValue("@emailValue", HttpUtility.UrlDecode(email));
                sqlCommand.Parameters.AddWithValue("@passwordValue", HttpUtility.UrlDecode(password));
                sqlCommand.Parameters.AddWithValue("@gamePlayedValue", Convert.ToInt32(game_played));

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
                Session["role"] = sqlDt.Rows[0]["role"];

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
                    gaming_username = sqlDt.Rows[i]["gaming_username"].ToString(),
                    game_played = Convert.ToInt32(sqlDt.Rows[i]["game_played"])
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
                Session["game_played"] = gameNum;
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
                Session["rank_tier"] = tier;
                Session["rank_division"] = division;
                Session["role"] = role;
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
                Session["rank_tier"] = rank;
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
            var user_id = Convert.ToInt32(Session["user_id"]);

            DataTable sqlDt = new DataTable("Users");

            string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
            string sqlSelect = "SELECT * FROM pj2.users WHERE game_played = 0 AND user_id = " + user_id + ";";

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
                    password = sqlDt.Rows[i]["password"].ToString()
                });
            }
            return user.ToArray();
        }

        // allows users to change their account info
        [WebMethod(EnableSession = true)]
        public string SaveInfo(string username, string email, string password, string gaming_username)
        {
            var user_id = Convert.ToInt32(Session["user_id"]);

            string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
            string sqlSelect = "UPDATE `pj2`.`users` SET `username` = (@usernameValue), `email` = (@emailValue), `password` = (@passwordValue), `gaming_username` = (@gamingUsernameValue) WHERE(`user_id` = " + user_id + ");";

            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            sqlCommand.Parameters.AddWithValue("@usernameValue", HttpUtility.UrlDecode(username));
            sqlCommand.Parameters.AddWithValue("@emailValue", HttpUtility.UrlDecode(email));
            sqlCommand.Parameters.AddWithValue("@passwordValue", HttpUtility.UrlDecode(password));
            sqlCommand.Parameters.AddWithValue("@gamingUsernameValue", HttpUtility.UrlDecode(gaming_username));

            sqlConnection.Open();

            try
            {
                sqlCommand.ExecuteNonQuery();
                return "Updated info";
            }

            catch (Exception e)
            {
                var e_str = e.ToString();
                return e_str;
            }
            sqlConnection.Close();
        }

        // list all players who play the same game
        [WebMethod(EnableSession = true)]
        public Users[] ViewPlayers()
        {
            var user_id = Convert.ToInt32(Session["user_id"]);
            var game_played = Convert.ToInt32(Session["game_played"]);

            DataTable sqlDt = new DataTable("Users");

            string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
            string sqlSelect = "SELECT * FROM pj2.users WHERE game_played = '" + game_played + "' AND user_id != " + user_id + ";";

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
                    game_played = Convert.ToInt32(sqlDt.Rows[i]["game_played"]),
                    rank_tier = sqlDt.Rows[i]["rank_tier"].ToString(),
                    rank_division = sqlDt.Rows[i]["rank_division"].ToString(),
                    role = sqlDt.Rows[i]["role"].ToString()
                });
            }
            return user.ToArray();
        }

        // Update a new gaming partner
        //[WebMethod(EnableSession = true)]
        //public string UpdatePartner(int partner_id)
        //{
        //    var user_id = Convert.ToInt32(Session["user_id"]);

        //    string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
        //    string sqlSelect = "UPDATE `pj2`.`users` SET `partner` = " + partner_id + " WHERE `user_id` = " + user_id + ";" +
        //                       "UPDATE `pj2`.`users` SET `partner` = " + user_id + " WHERE `user_id` = " + partner_id + ";";

        //    MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
        //    MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

        //    sqlConnection.Open();

        //    try
        //    {
        //        sqlCommand.ExecuteNonQuery();
        //        return "Updated partner id";
        //    }

        //    catch (Exception e)
        //    {
        //        var e_str = e.ToString();
        //        return e_str;
        //    }
        //    sqlConnection.Close();
        //}

        
        [WebMethod(EnableSession = true)]
        public string SendRequest( int partner_id, string date, string time)
        {
            var user_id = Convert.ToInt32(Session["user_id"]);

            string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
            string sqlSelect = "INSERT INTO `pj2`.`requests` (`request_user`, `request_partner`, `date`, `time`, `status`) VALUES('" + user_id + "', '" + partner_id + "', '" + date + "', '" + time + "', 'pending');";

            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            sqlConnection.Open();

            try
            {
                sqlCommand.ExecuteNonQuery();
                return "pending request";
            }

            catch (Exception e)
            {
                var e_str = e.ToString();
                return e_str;
            }
            sqlConnection.Close();
        }

        // list all info of requests
        [WebMethod(EnableSession = true)]
        public Requests[] ViewRequests()
        {
            var user_id = Convert.ToInt32(Session["user_id"]);

            DataTable sqlDt = new DataTable("Requests");

            string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
            string sqlSelect = "SELECT request_id, request_user, request_partner, date, time, status, username, rank_tier, rank_division, role FROM pj2.requests, pj2.users WHERE pj2.requests.status = 'pending' AND pj2.requests.request_user = pj2.users.user_id AND pj2.requests.request_partner = '" + user_id + "' ORDER BY date;";

            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
            sqlDa.Fill(sqlDt);

            List<Requests> request = new List<Requests>();
            for (int i = 0; i < sqlDt.Rows.Count; i++)
            {
                request.Add(new Requests
                {
                    request_id = Convert.ToInt32(sqlDt.Rows[i]["request_id"]),
                    request_user = Convert.ToInt32(sqlDt.Rows[i]["request_user"]),
                    request_partner = Convert.ToInt32(sqlDt.Rows[i]["request_partner"]),
                    date = sqlDt.Rows[i]["date"].ToString(),
                    time = sqlDt.Rows[i]["time"].ToString(),
                    status = sqlDt.Rows[i]["status"].ToString(),
                    username = sqlDt.Rows[i]["username"].ToString(),
                    request_user_tier = sqlDt.Rows[i]["rank_tier"].ToString(),
                    request_user_division = sqlDt.Rows[i]["rank_division"].ToString(),
                    request_user_role = sqlDt.Rows[i]["role"].ToString()
                });
            }
            return request.ToArray();
        }

        [WebMethod(EnableSession = true)]
        public void AcceptRequest(int request_id)
        {
            var user_id = Convert.ToInt32(Session["user_id"]);

            string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
            string sqlSelect = "UPDATE `pj2`.`requests` SET `status` = 'accept' WHERE(`request_id` = " + request_id + ");";

            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            sqlConnection.Open();

            try
            {
                sqlCommand.ExecuteNonQuery();
            }

            catch (Exception e)
            {
            }
            sqlConnection.Close();
        }

        [WebMethod(EnableSession = true)]
        public void DenyRequest(int request_id)
        {
            var user_id = Convert.ToInt32(Session["user_id"]);

            string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
            string sqlSelect = "UPDATE `pj2`.`requests` SET `status` = 'deny' WHERE(`request_id` = " + request_id + ");";

            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            sqlConnection.Open();

            try
            {
                sqlCommand.ExecuteNonQuery();
            }

            catch (Exception e)
            {
            }
            sqlConnection.Close();
        }

        // list all info of requests
        [WebMethod(EnableSession = true)]
        public Schedules[] ViewSchedulesUser()
        {
            var user_id = Convert.ToInt32(Session["user_id"]);

            DataTable sqlDt = new DataTable("Schedules");

            string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
            string sqlSelect = "SELECT request_id, request_user, request_partner, date, time, status, rank_tier, rank_division, role, pj2.users.gaming_username FROM pj2.requests, pj2.users WHERE pj2.requests.request_user = pj2.users.user_id AND pj2.requests.status = 'accept' AND pj2.requests.request_partner = '" + user_id + "' ORDER BY date;";

            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
            sqlDa.Fill(sqlDt);

            List<Schedules> schedule = new List<Schedules>();
            for (int i = 0; i < sqlDt.Rows.Count; i++)
            {
                schedule.Add(new Schedules
                {
                    request_id = Convert.ToInt32(sqlDt.Rows[i]["request_id"]),
                    request_user = Convert.ToInt32(sqlDt.Rows[i]["request_user"]),
                    request_partner = Convert.ToInt32(sqlDt.Rows[i]["request_partner"]),
                    date = sqlDt.Rows[i]["date"].ToString(),
                    time = sqlDt.Rows[i]["time"].ToString(),
                    status = sqlDt.Rows[i]["status"].ToString(),
                    request_gaming_username = sqlDt.Rows[i]["gaming_username"].ToString(),
                    request_user_tier = sqlDt.Rows[i]["rank_tier"].ToString(),
                    request_user_division = sqlDt.Rows[i]["rank_division"].ToString(),
                    request_user_role = sqlDt.Rows[i]["role"].ToString()
                });
            }
            return schedule.ToArray();
        }

        // display all players'info & scheduled playing time
        [WebMethod(EnableSession = true)]
        public Schedules[] ViewSchedulesPartner()
        {
            var user_id = Convert.ToInt32(Session["user_id"]);

            DataTable sqlDt = new DataTable("Schedules");

            string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
            string sqlSelect = "SELECT request_id, request_user, request_partner, date, time, status, rank_tier, rank_division, role, pj2.users.gaming_username FROM pj2.requests, pj2.users WHERE pj2.requests.request_partner = pj2.users.user_id AND pj2.requests.status = 'accept' AND pj2.requests.request_user = '" + user_id + "' ORDER BY date;";

            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            MySqlDataAdapter sqlDa = new MySqlDataAdapter(sqlCommand);
            sqlDa.Fill(sqlDt);

            List<Schedules> schedule = new List<Schedules>();
            for (int i = 0; i < sqlDt.Rows.Count; i++)
            {
                schedule.Add(new Schedules
                {
                    request_id = Convert.ToInt32(sqlDt.Rows[i]["request_id"]),
                    request_gaming_username = sqlDt.Rows[i]["gaming_username"].ToString(),
                    request_partner_tier = sqlDt.Rows[i]["rank_tier"].ToString(),
                    request_partner_division = sqlDt.Rows[i]["rank_division"].ToString(),
                    request_partner_role = sqlDt.Rows[i]["role"].ToString(),
                    date = sqlDt.Rows[i]["date"].ToString(),
                    time = sqlDt.Rows[i]["time"].ToString()
                });
            }
            return schedule.ToArray();
        }
        
        // reset info
        [WebMethod(EnableSession = true)]
        public void ResetInfo()
        {
            var user_id = Convert.ToInt32(Session["user_id"]);

            string sqlConnectString = System.Configuration.ConfigurationManager.ConnectionStrings["myDB"].ConnectionString;
            string sqlSelect = "UPDATE `pj2`.`users` SET `game_played` = 0, `rank_tier` = null, `rank_division` = null, `role` = null WHERE(`user_id` = " + user_id + ");";

            MySqlConnection sqlConnection = new MySqlConnection(sqlConnectString);
            MySqlCommand sqlCommand = new MySqlCommand(sqlSelect, sqlConnection);

            sqlConnection.Open();

            try
            {
                sqlCommand.ExecuteNonQuery();
            }

            catch (Exception e)
            {
            }
            sqlConnection.Close();
        }

    }
}
