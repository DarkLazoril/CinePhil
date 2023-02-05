using System;
using System.ComponentModel.DataAnnotations;

namespace MovieApp.Models
{
    public class Watch
    {
        [Key]
        public int WatchId {get;set;}
        public int UserId {get;set;} 
        public User UserWhoWatches {get;set;} 
        public int MovieId {get;set;} 
        // public Result WatchedMovie {get;set;} 
        public DateTime CreatedAt {get;set;} = DateTime.Now;
        public DateTime UpdatedAt {get;set;} = DateTime.Now;

    }
}