//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MSOCore
{
    using System;
    using System.Collections.Generic;
    
    public partial class Game
    {
        public Game()
        {
            this.Events = new HashSet<Event>();
        }
    
        public string Code { get; set; }
        public string Mind_Sport { get; set; }
        public string Contacts { get; set; }
        public string Equipment { get; set; }
        public string Rules { get; set; }
        public byte[] Logo { get; set; }
        public byte[] SSMA_TimeStamp { get; set; }
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string FindOutMoreLink { get; set; }
        public string OnlineSiteName { get; set; }
        public string OnlineSiteUrl { get; set; }

        public virtual ICollection<Event> Events { get; set; }
        public virtual GameCategory GameCategory { get; set; }
    }
}
