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
    
    public partial class Seeding
    {
        public int Id { get; set; }
        public int ContestantId { get; set; }
        public string EventCode { get; set; }
        public Nullable<int> Score { get; set; }
        public Nullable<int> Rank { get; set; }
    
        public virtual Contestant Name { get; set; }
    }
}
