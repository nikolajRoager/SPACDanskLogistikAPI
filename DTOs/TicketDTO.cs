using DanskLogistikAPI.Models;

namespace DanskLogistikAPI.DTOs
{
    /// <summary>
    /// A special kind of DTO, exclusively created for the user to view a ticked item, not meant to be convertable back into a ticket
    /// This INCLUDES a list of TransitStepDTOs
    /// </summary>
    public class TicketDTO
    {

        public int id { get; set; }

        /// <summary>
        /// What is the CURRENT status of this ticked?
        /// </summary>
        public Ticket.StatusType status { get; set; }
        /// <summary>
        /// Where 
        /// </summary>
        public int DestinationId { get; set; }
        /// <summary>
        /// Where 
        /// </summary>
        public string DestinationName { get; set; } = null!;

        /// <summary>
        /// How many places do we pass through?
        /// </summary>
        public int currentStep { get; set; }


        public List<TransitStepDTO> Steps { get; set; }

        public TicketDTO(Ticket ticket)
        {
            this.id = ticket.id;
            this.status = ticket.status;
            this.DestinationId = ticket.Destination.Id;
            this.DestinationName = ticket.Destination.Name;

            this.Steps = new List<TransitStepDTO>();
            //Loop  through the single linked-list of steps in the ticked, and built and array
            currentStep = 0;
            int stepCounter = 0;
            for (TransitStep? Step = ticket.FirstStep; Step != null && stepCounter < ticket.Steps; Step = Step.next, ++stepCounter)
            {
                if (Step.Id == ticket.CurrentStep.Id)
                {
                    currentStep = stepCounter;
                }
                Steps.Add(
                    new TransitStepDTO {
                        Id = Step.Id,
                        FromName = Step.Via.Start.Name,
                        FromID = Step.Via.Start.Id,
                        ToName = Step.Via.End.Name,
                        ToID = Step.Via.End.Id,
                        Mode = Step.Via.Connection.mode,
                        ConnectionName= Step.Via.Connection.Name,
                        Departure=Step.Departure}
                    );
            }

        }
    }
}
