using MediatR;

namespace ClaimsModule.Application.Common.Interfaces;

public interface ICommand : IRequest { }

public interface ICommand<out TResponse> : IRequest<TResponse> { }
