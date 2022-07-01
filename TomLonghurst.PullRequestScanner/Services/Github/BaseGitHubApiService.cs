using TomLonghurst.PullRequestScanner.Http;

namespace TomLonghurst.PullRequestScanner.Services.Github;

internal abstract class BaseGitHubApiService
{
    protected readonly GithubHttpClient GithubGithubHttpClient;

    protected BaseGitHubApiService(GithubHttpClient githubHttpClient)
    {
        GithubGithubHttpClient = githubHttpClient;
    }
    
    protected async Task<List<T>> Get<T>(string path)
    {
        int arrayCount;
        var iteration = 1;

        var list = new List<T>();
        do
        {
            var arrayResponse = await GithubGithubHttpClient.Get<List<T>>($"{path}?per_page=100&page={iteration}");
            arrayCount = arrayResponse.Count;
            iteration++;
            list.AddRange(arrayResponse);
        } while (arrayCount >= 100);

        return list;
    }
}