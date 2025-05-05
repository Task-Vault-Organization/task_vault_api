using AutoMapper;
using Microsoft.AspNetCore.Http;
using TaskVault.Business.Shared.Exceptions;
using TaskVault.Contracts.Shared.Abstractions.Services;
using TaskVault.Contracts.Shared.Dtos;
using TaskVault.DataAccess.Entities;
using TaskVault.DataAccess.Repositories.Abstractions;

namespace TaskVault.Business.Shared.Services;

public class UsersService : IUsersService
{
    private readonly IExceptionHandlingService _exceptionHandlingService;
    private readonly IRepository<User> _userRepository;
    private readonly IMapper _mapper;

    public UsersService(IExceptionHandlingService exceptionHandlingService, IRepository<User> userRepository, IMapper mapper)
    {
        _exceptionHandlingService = exceptionHandlingService;
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<SearchUsersResponseDto> SearchUsersAsync(string userEmail, string searchField)
    {
        return await _exceptionHandlingService.ExecuteWithExceptionHandlingAsync(async () =>
        {
            var foundUser = (await _userRepository.FindAsync((u) => u.Email == userEmail)).FirstOrDefault();
            if (foundUser == null)
            {
                throw new ServiceException(StatusCodes.Status404NotFound, "User not found");
            }

            if (searchField.Length < 3)
            {
                throw new ServiceException(StatusCodes.Status400BadRequest,
                    "Search field length must be at least 3 characters");
            }

            var usersFound = await _userRepository
                .FindAsync(u =>
                    u.Email.ToLower().Substring(0,
                        u.Email.IndexOf("@") > 0 ? u.Email.IndexOf("@") : u.Email.Length
                    ).Contains(searchField.ToLower())
                );

            var enumerable = usersFound as User[] ?? usersFound.ToArray();
            var result = enumerable.Select(u => _mapper.Map<GetUserDto>(u));


            var getUserDtos = result as GetUserDto[] ?? result.ToArray();
            var successMessage = getUserDtos.Any() ? "Successfully searched users" : "No user found";
            return SearchUsersResponseDto.Create(successMessage, getUserDtos);
        }, "Error when searching for users");
    }
}