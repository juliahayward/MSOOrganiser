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
    
    public partial class WomenNotInWomensPentamind
    {
        public int OlympiadId { get; set; }
        public int ContestantId { get; set; }
        public int Id { get; set; }
    
        public virtual Contestant Name { get; set; }
        public virtual Olympiad_Info Olympiad_Info { get; set; }
    }
}
