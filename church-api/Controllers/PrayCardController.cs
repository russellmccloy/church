using AutoMapper;
using church_api.Models;
using church_api.Repositories.Abstractions;
using church_api.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;

namespace church_api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [RequiredScope(RequiredScopesConfigurationKey = "AzureAd:Scopes")]
    public class PrayerCardController : ApiControllerBase
    {
        private readonly IPrayerCardRepository _prayerCardRepository;
        private readonly IMapper _mapper;
        private readonly IFileUploader _fileUploader;

        public PrayerCardController(IPrayerCardRepository prayerCardRepository, IMapper mapper, IFileUploader fileUploader)
        {
            _prayerCardRepository = prayerCardRepository;
            _mapper = mapper;
            _fileUploader = fileUploader;
        }

        [HttpGet("{userId}")]
        public async Task<ActionResult> Index(Guid userId, CancellationToken cancellationToken)
        {
            var tesmp = GetUserRoles();

            var prayerCards = await _prayerCardRepository.GetPrayCardsAsync(userId, cancellationToken);

            // Use AutoMapper to map the entities to the view models
            var prayerCardViewModels = _mapper.Map<IEnumerable<PrayerCardViewModel>>(prayerCards);

            // Return the list as JSON
            return Ok(prayerCardViewModels);
        }

        // Add a new PrayerCard (POST)
        [HttpPost]
        public async Task<ActionResult> Create([FromBody] PrayerCardViewModel prayerCardViewModel, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);  // Return validation errors if the model is not valid
            }
            // Map the view model to an entity (assuming you have a PrayerCard entity)
            var prayerCardEntity = _mapper.Map<PrayerCard>(prayerCardViewModel);

            // Call the repository to save the prayer card (assuming the repository has an Add method)
            var createdPrayerCard = await _prayerCardRepository.AddPrayerCardAsync(prayerCardEntity, cancellationToken);

            // Upload the image to Azure Blob Storage
            if (!string.IsNullOrEmpty(prayerCardViewModel.ImagePath))
            {
                // Assuming ImagePath contains the local file path or a stream
                var blobName = $"{createdPrayerCard.UserId}/{Path.GetFileName(prayerCardViewModel.ImagePath)}"; // Generate a unique blob name
                var uploadResponse = await _fileUploader.UploadFileAsync(prayerCardViewModel.ImagePath, blobName);

                // Update the prayer card with the blob URL
                createdPrayerCard.ImagePath = blobName;

                // Optionally, save the updated prayer card entity with the image path
                // (if your repository supports an update operation, otherwise this can be done later)
                await _prayerCardRepository.AddPrayerCardAsync(createdPrayerCard, cancellationToken);
            }

            var returnedPrayerCardViewModel = _mapper.Map<PrayerCardViewModel>(createdPrayerCard);

            // Return the created prayer card with a 201 Created status code
            return CreatedAtAction(nameof(Index), new { userId = returnedPrayerCardViewModel.UserId }, returnedPrayerCardViewModel);
        }
    }
}
