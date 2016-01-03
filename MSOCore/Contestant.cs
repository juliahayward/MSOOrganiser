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
    
    public partial class Contestant
    {
        public Contestant()
        {
            this.Entrants = new HashSet<Entrant>();
            this.Payments = new HashSet<Payment>();
            this.Arbiters = new HashSet<Arbiter>();
        }
    
        public int Mind_Sport_ID { get; set; }
        public string Title { get; set; }
        public string Lastname { get; set; }
        public string Firstname { get; set; }
        public string Initials { get; set; }
        public bool Male { get; set; }
        public string Nationality { get; set; }
        public string DayPhone { get; set; }
        public string EvePhone { get; set; }
        public string Fax { get; set; }
        public string email { get; set; }
        public Nullable<bool> No_News { get; set; }
        public Nullable<System.DateTime> DateofBirth { get; set; }
        public Nullable<bool> Concessional { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string County { get; set; }
        public string PostCode { get; set; }
        public string Country { get; set; }
        public string BCFCode { get; set; }
        public string FIDECode { get; set; }
        public string Notes { get; set; }
        public Nullable<System.DateTime> date { get; set; }
        public byte[] SSMA_TimeStamp { get; set; }
    
        public virtual ICollection<Entrant> Entrants { get; set; }
        public virtual ICollection<Payment> Payments { get; set; }
        public virtual ICollection<Arbiter> Arbiters { get; set; }
    }
}
