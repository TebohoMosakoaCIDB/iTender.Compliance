using System;
using System.Collections.Generic;
using System.Text;

namespace iTender.Compliance.Domain.Entities
{
    public class PublicHoliday : BaseEntity
    {
        public DateOnly Date { get; set; }

        public string Name { get; set; } = string.Empty;

        /// <summary>True if this row exists only because the actual holiday fell on a Sunday
        /// and the Public Holidays Act moves observance to the following Monday.</summary>
        public bool IsObservedShift { get; set; }
    }
}
