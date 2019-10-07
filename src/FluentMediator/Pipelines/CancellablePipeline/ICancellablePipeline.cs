using System.Threading;
using System.Threading.Tasks;

namespace FluentMediator
{
    public interface ICancellablePipeline
    {
        Task PublishAsync(object request, CancellationToken cancellationToken);
        Task<Response> SendAsync<Response>(object request, CancellationToken cancellationToken);
        IMediator Build();
    }
}