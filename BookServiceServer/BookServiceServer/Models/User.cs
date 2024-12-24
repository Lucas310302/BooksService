﻿using System.ComponentModel.DataAnnotations;

namespace BookServiceServer.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; } = "User";
        public DateTime CreatedAt { get; set; }
    }
}