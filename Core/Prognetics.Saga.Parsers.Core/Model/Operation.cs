namespace Prognetics.Saga.Parsers.Core.Model
{
    public class Operation
    {
        public int StepNumber { get; set; }

        public string EventName { get; set; }

        public string EventCompletionName { get; set; }

        public string CompensationEventName { get; set; }
    }
}
