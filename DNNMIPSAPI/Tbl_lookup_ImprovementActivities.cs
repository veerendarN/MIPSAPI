//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DNNMIPSAPI
{
    using System;
    using System.Collections.Generic;
    
    public partial class Tbl_lookup_ImprovementActivities
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Tbl_lookup_ImprovementActivities()
        {
            this.Tbl_IA_Data = new HashSet<Tbl_IA_Data>();
        }
    
        public short Id { get; set; }
        public string Description { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Tbl_IA_Data> Tbl_IA_Data { get; set; }
    }
}
