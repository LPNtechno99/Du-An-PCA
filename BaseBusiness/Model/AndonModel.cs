
using System;
namespace BMS.Model
{
	public partial class AndonModel : BaseModel
	{
		public int ID {get; set;}
		
		public int ShiftID {get; set;}
		
		public DateTime? ShiftStartTime {get; set;}
		
		public DateTime? ShiftEndTime {get; set;}
		
		public DateTime? StartTimeBreak {get; set;}
		
		public DateTime? EndTimeBreak {get; set;}
		
		public int TotalSeconds {get; set;}
		
		public DateTime? DateLR {get; set;}
		
		public int Takt {get; set;}
		
		public int QtyPlanDay {get; set;}
		
		public int QtyPlanCurrent {get; set;}
		
		public int QtyActual {get; set;}
		
		public int QtyActualNG {get; set;}
		
		public int QtyDelay {get; set;}
		
		public int QtyNG {get; set;}
		
		public bool IsStart {get; set;}
		
	}
}
	