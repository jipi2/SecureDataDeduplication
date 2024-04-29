
using FileStorageApp.Server.Entity;
using FileStorageApp.Server.Repositories;
using FileStorageApp.Shared.Dto;

namespace FileStorageApp.Server.Services
{
    public class LabelService
    {

        UserRepository _userRepo;
        UserFileRepository _userFileRepo;
        LabelRepository _labelRepo;

        public LabelService(UserRepository userRepo, UserFileRepository userFileRepo, LabelRepository labelRepo)
        {
            _userRepo = userRepo;
            _userFileRepo = userFileRepo;
            _labelRepo = labelRepo;
        }

        public async Task CreateLabel(string userId, string labelName)
        {
            User? user = await _userRepo.GetUserById(Convert.ToInt32(userId)); 
            if(user == null)
                throw new Exception("User not found");
            Label? label = await _labelRepo.GetLabelByUserIdAndName(user.Id, labelName);
            if(label != null)
                throw new Exception("Label already exists");
            Label newLabel = new Label()
            {
                Name = labelName,
                UserId = user.Id
            };
            await _labelRepo.SaveLabel(newLabel);

        }

        public async Task AddLabelToFile(string userId, AddLabelToFileDto dto)
        {
            User? user = await _userRepo.GetUserById(Convert.ToInt32(userId));
            if (user == null)
                throw new Exception("User not found");
            Label? label = await _labelRepo.GetLabelByUserIdAndName(user.Id, dto.labelName);
            if (label == null)
                throw new Exception("Label already exists");

            UserFile? userFile = await _userFileRepo.GetUserFileByUserIdAndFileName(user.Id, dto.fileName);
            if (userFile == null)
                throw new Exception("File not found");

            if (label.UserFiles.Contains(userFile) == true)
            {
                throw new Exception("The file is allready in that category!");
            }
            label.UserFiles.Add(userFile);
            await _labelRepo.UpdateLabel(label);
        }

        public async Task<LabelsDto> GetLabelsForUser(string userId)
        {
            LabelsDto labelsDto = new LabelsDto();

            User? user = await _userRepo.GetUserById(Convert.ToInt32(userId));
            if (user == null)
                throw new Exception("User not found");
            List<Label> labels = await _labelRepo.GetLabelsByUserId(user.Id);
            if (labels == null)
            {
                labelsDto.list = new List<LabelFileNames>();
                return labelsDto; 
            }

            labelsDto.list = new List<LabelFileNames>();
            foreach (var label in labels)
            {
                var fileNames = label.UserFiles.Count != 0 ? label.UserFiles.Select(f => f.FileName).ToList() : new List<string>();
                labelsDto.list.Add(
                    new LabelFileNames()
                    {
                        labelName = label.Name,
                        fileNames = label.UserFiles.Count != 0 ? label.UserFiles.Select(f => f.FileName).ToList() : new List<string>()
                    }
                    );              
            }
            return labelsDto;
        }

        public async Task RemoveLabelFile(string userId, RemoveLabelFileDto dto)
        {
            User? user = await _userRepo.GetUserById(Convert.ToInt32(userId));
            if (user == null)
                throw new Exception("User not found");
            Label? label = await _labelRepo.GetLabelByUserIdAndName(user.Id, dto.labelName);
            if (label == null)
                throw new Exception("Label already exists");

            UserFile? userFile = await _userFileRepo.GetUserFileByUserIdAndFileName(user.Id, dto.fileName);
            if (userFile == null)
                throw new Exception("File not found");

            if (label.UserFiles.Contains(userFile) == true)
            {
                label.UserFiles.Remove(userFile);
                await _labelRepo.UpdateLabel(label);
            }

        }

        public async Task RemoveLabel(string userId, string labelName)
        {
            User? user = await _userRepo.GetUserById(Convert.ToInt32(userId));
            if (user == null)
                throw new Exception("User not found");
            Label? label = await _labelRepo.GetLabelByUserIdAndName(user.Id, labelName);
            if (label == null)
                throw new Exception("Label already exists");
            await _labelRepo.RemoveLabel(label);
        }
    }
}
