﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SoftwareOptimization.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SoftwareOptimization.Controllers {
    public class UsersController : Controller {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration configuration;
        private readonly string connectionString;
        public UsersController(ILogger<HomeController> logger, IConfiguration config) {
            configuration = config;
            connectionString = configuration.GetConnectionString("DefaultConnection");
            _logger = logger;
        }

        [HttpGet]
        public IActionResult SignIn() {
           if(User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignIn(string username, string password) {
            if(string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return View(); // bad request
            using(var connection = new SqlConnection(connectionString))
            {
                using(var command = connection.CreateCommand())
                {                                              
                   
                    // **** Using pure SQL Query vulnerable to SQL Injection
                    command.CommandType = System.Data.CommandType.Text;
                    command.CommandText = $"EXEC dbo.Users_SignIn @Password = '{password}', @State = @State OUT, @UserId = @UserId OUT, @Username = '{username}'";
                    command.Parameters.Add("@userid", System.Data.SqlDbType.Int).Direction = System.Data.ParameterDirection.Output;
                    command.Parameters.Add("@state", System.Data.SqlDbType.Int).Direction = System.Data.ParameterDirection.Output;

                    connection.Open();
                    await command.ExecuteNonQueryAsync();
                    connection.Close();
                    int state = Convert.ToInt32(command.Parameters["@State"].Value);
                    int userId = 0;
                    switch(state)
                    {
                        case 1:
                            userId = Convert.ToInt32(command.Parameters["@UserId"].Value);
                            break;
                        case 2: // bad request
                            return View();
                        case 3: // user not found
                            return View();
                        default:
                            throw new Exception($"Unhandled state: {state}");
                    }

                    await SignUser(username, userId);

                    return RedirectToAction("Index", "Home");
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> SignIn2(string username, string password) {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return View("SignIn");// bad request
            using (var connection = new SqlConnection(connectionString))
            {
                using (var command = connection.CreateCommand())
                {

                    // **** Using store procedures for preventing SQL Injection
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.CommandText = "dbo.Users_SignIn";
                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@password", password);
                    command.Parameters.Add("@userid", System.Data.SqlDbType.Int).Direction = System.Data.ParameterDirection.Output;
                    command.Parameters.Add("@state", System.Data.SqlDbType.Int).Direction = System.Data.ParameterDirection.Output;

                    connection.Open();
                    await command.ExecuteNonQueryAsync();
                    connection.Close();
                    int state = Convert.ToInt32(command.Parameters["@State"].Value);
                    int userId = 0;
                    switch (state)
                    {
                        case 1:
                            userId = Convert.ToInt32(command.Parameters["@UserId"].Value);
                            break;
                        case 2: // bad request
                            return View("SignIn");
                        case 3: // user not found
                            return View("SignIn");
                        default:
                            throw new Exception($"Unhandled state: {state}");
                    }

                    await SignUser(username, userId);

                    return RedirectToAction("Index", "Home");
                }
            }
        }

        private async Task SignUser(string username, int userId) {
            // Initial FormsAuthentication
            var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, $"{userId}|{username}"),
                        new Claim(ClaimTypes.Role, "Users")
                    };
            var claimIdentity = new ClaimsIdentity(claims, "Users");
            var claimPrinciple = new ClaimsPrincipal(claimIdentity);
            var authenticationProperty = new AuthenticationProperties
            {
                IsPersistent = true
            };
            await HttpContext.SignInAsync(claimPrinciple, authenticationProperty);
            // End initial FormsAuthentication
        }

        [HttpGet]
        public IActionResult SignUp() {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignUp(string username, string password) {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return View(); // bad request
            using (var connection = new SqlConnection(connectionString))
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.CommandText = "dbo.Users_SignUp";
                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@password", password);
                    command.Parameters.Add("@userid", System.Data.SqlDbType.Int).Direction = System.Data.ParameterDirection.Output;
                    command.Parameters.Add("@state", System.Data.SqlDbType.Int).Direction = System.Data.ParameterDirection.Output;
                    connection.Open();
                    await command.ExecuteNonQueryAsync();
                    connection.Close();
                    int state = Convert.ToInt32(command.Parameters["@State"].Value);
                    int userId = 0;
                    switch (state)
                    {
                        case 1:
                            userId = Convert.ToInt32(command.Parameters["@UserId"].Value);
                            break;
                        case 2: // bad request
                            return View();
                        case 3: // username is duplicated
                            return View();
                        default:
                            throw new Exception($"Unhandled state: {state}");
                    }

                    await SignUser(username, userId);

                    return RedirectToAction("Index", "Home");
                }
            }
        }
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> SignOut() {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
