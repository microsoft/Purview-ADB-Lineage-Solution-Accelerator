
namespace Function.Domain.Services
{
    public interface IOlFilter
    {
        bool FilterOlMessage(string strRequest);
        string GetJobNamespace(string strRequest);
    }
}