namespace TierList.Application.Features.Votes.DTOs;

public record VoteResultDto(int LikesCount, int DislikesCount, int? UserVote);
