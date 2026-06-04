using AutoMapper;
using HMS.DAL.Contracts;
using HMS.DAL.Models.Enums.PatientEnums;
using HMS.DAL.Models.PatientModule;
using HMS.BLL.Services.Exceptions;
using HMS.BLL.Services.Specifications.PatientModule;
using HMS.BLL.Shared.Dtos.PatientModule.EmergencyContactsDtos;
using HMS.BLL.ServicesAbstraction.Contracts;

namespace HMS.BLL.Services.Implementations.PatientModule
{
    public class EmergencyContactService (IUnitOfWork _unitOfWork , IMapper _mapper) : IEmergencyContactService
    {
        public async Task<EmergencyContactResultDto> AddEmergencyContactAsync(int patientId, CreateEmergencyContactDto contactDto)
        {
            // STEP 1: Verify patient exists
            var patientRepository = _unitOfWork.GetRepository<Patient, int>();
            var patient = await patientRepository.GetByIdAsync(patientId);

            // Throw exception instead of returning null 
            if (patient is null)
                throw new NotFoundException(nameof(Patient), patientId);

            // Check if patient is active (Business Rule)
            if (patient.Status != PatientStatus.Active)
                throw new BusinessRuleException("Cannot add emergency contact to an inactive patient.");



            // STEP 2: Map DTO to Entity
            var emergencyContact = _mapper.Map<EmergencyContact>(contactDto);
            emergencyContact.PatientId = patientId;
            emergencyContact.CreatedAt = DateTime.UtcNow;

            // STEP 3: Get emergency contact repository and add
            var contactRepository = _unitOfWork.GetRepository<EmergencyContact, int>();
            await contactRepository.AddAsync(emergencyContact);

            // STEP 4: Save changes
            await _unitOfWork.SaveChangesAsync();

            // STEP 5: Return result DTO
            return _mapper.Map<EmergencyContactResultDto>(emergencyContact);
        }

        public async Task<IEnumerable<EmergencyContactResultDto>> GetEmergencyContactsAsync(int patientId)
        {
            var patientRepository = _unitOfWork.GetRepository<Patient, int>();
            var patient = await patientRepository.GetByIdAsync(patientId);

            if (patient is null)
                throw new NotFoundException(nameof(Patient), patientId);

            var contactRepository = _unitOfWork.GetRepository<EmergencyContact, int>();
            var spec = new PatientEmergencyContactSpecification(patientId);
            var patientContacts = await contactRepository.GetAllAsync(spec);

            return _mapper.Map<IEnumerable<EmergencyContactResultDto>>(patientContacts);
        }
        public async Task<EmergencyContactResultDto> UpdateEmergencyContactAsync(int patientId, int contactId, UpdateEmergencyContactDto contactDto)
        {
            // STEP 1: Get emergency contact repository
            var contactRepository = _unitOfWork.GetRepository<EmergencyContact, int>();
            var emergencyContact = await contactRepository.GetByIdAsync(contactId);

            // STEP 2: Verify contact exists and belongs to patient
            
                //if (emergencyContact is null || emergencyContact.PatientId != patientId)
                //    return null!;

            if (emergencyContact is null)
                throw new NotFoundException(nameof(EmergencyContact), contactId);

            if (emergencyContact.PatientId != patientId)
                throw new BusinessRuleException($"Emergency contact with ID {contactId} does not belong to patient {patientId}.");


            // STEP 3: Update fields if provided
            if (!string.IsNullOrEmpty(contactDto.Name))
                emergencyContact.Name = contactDto.Name;

            if (!string.IsNullOrEmpty(contactDto.Relationship))
                emergencyContact.Relationship = contactDto.Relationship;

            if (!string.IsNullOrEmpty(contactDto.Phone))
                emergencyContact.Phone = contactDto.Phone;

            if (!string.IsNullOrEmpty(contactDto.Email))
                emergencyContact.Email = contactDto.Email;

            if (contactDto.IsPrimaryContact.HasValue)
                emergencyContact.IsPrimaryContact = contactDto.IsPrimaryContact.Value;

            if (!string.IsNullOrEmpty(contactDto.Notes))
                emergencyContact.Notes = contactDto.Notes;

            // Update timestamp
            emergencyContact.UpdatedAt = DateTime.UtcNow;

            // STEP 4: Mark as modified
            contactRepository.Update(emergencyContact);

            // STEP 5: Save changes
            await _unitOfWork.SaveChangesAsync();

            // STEP 6: Return updated DTO
            return _mapper.Map<EmergencyContactResultDto>(emergencyContact);
        }

        public async Task<bool> DeleteEmergencyContactAsync(int patientId, int contactId)
        {
            // STEP 1: Get emergency contact repository
            var contactRepository = _unitOfWork.GetRepository<EmergencyContact, int>();
            var emergencyContact = await contactRepository.GetByIdAsync(contactId);

            // STEP 2: Verify contact exists and belongs to patient
              if (emergencyContact is null)
                throw new NotFoundException(nameof(EmergencyContact), contactId);

            if (emergencyContact.PatientId != patientId)
                throw new BusinessRuleException($"Emergency contact with ID {contactId} does not belong to patient {patientId}.");

            // STEP 3: Delete contact
            contactRepository.Delete(emergencyContact);

            // STEP 4: Save changes
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}
