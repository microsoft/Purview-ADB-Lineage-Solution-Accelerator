using System.Threading.Tasks;
using Function.Domain.Models.OL;

namespace Function.Domain.Helpers.Parser
{
    public interface IValidateOlEvent
    {
        public bool Validate(Event olEvent);
    }
}