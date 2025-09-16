using CLI.UI.ManageUsers;
using RepositoryContracts;

namespace CLI.UI.ManagePosts;

public class CreatePostView
{
    private readonly IPostRepository  postRepository;

    public CreatePostView(IPostRepository postRepository)
    {
        this.postRepository = postRepository;
    }
}