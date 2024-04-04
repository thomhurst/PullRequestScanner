namespace TomLonghurst.PullRequestScanner.GitHub.Services;

using Http;

internal abstract class BaseGitHubApiService
{
    private readonly GithubHttpClient githubHttpClient;

    protected BaseGitHubApiService(GithubHttpClient githubHttpClient)
    {
        this.githubHttpClient = githubHttpClient;
    }

    protected async Task<List<T>> Get<T>(string path)
    {
        int arrayCount;
        var iteration = 1;

        var list = new List<T>();
        do
        {
            var arrayResponse = await githubHttpClient.Get<List<T>>($"{path}?per_page=100&page={iteration}");

            if (arrayResponse?.Count is null or 0)
            {
                break;
            }

            arrayCount = arrayResponse.Count;
            iteration++;
            list.AddRange(arrayResponse);
        }
        while (arrayCount >= 100);

        return list;
    }
}