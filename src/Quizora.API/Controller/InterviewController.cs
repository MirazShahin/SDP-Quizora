using Microsoft.AspNetCore.Mvc;
using Quizora.Application.Common;
using Quizora.Application.Interfaces;

namespace Quizora.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InterviewController : ControllerBase
{
    private readonly IInterviewRepository _interviewRepository;

    public InterviewController(IInterviewRepository interviewRepository)
    {
        _interviewRepository = interviewRepository;
    }

    [HttpGet("topics")]
    public async Task<ActionResult<Result<object>>> GetTopics()
    {
        var topics = await _interviewRepository.GetAllTopicsAsync();

        var result = topics.Select(t => new
        {
            t.Id,
            t.Name,
            t.Description,
            t.Icon,
            t.Order
        });

        return Result<object>.Success(result);
    }

    [HttpGet("topics/{topicId}/qas")]
    public async Task<ActionResult<Result<object>>> GetQAs(Guid topicId)
    {
        var topic = await _interviewRepository.GetTopicWithQAsAsync(topicId);
        if (topic == null)
            return Result<object>.Failure("Topic not found");

        var qas = topic.QAs.Select(q => new
        {
            q.Id,
            q.Question,
            q.Answer,
            q.Order
        });

        return Result<object>.Success(new
        {
            TopicName = topic.Name,
            TopicDescription = topic.Description,
            QAs = qas
        });
    }
}