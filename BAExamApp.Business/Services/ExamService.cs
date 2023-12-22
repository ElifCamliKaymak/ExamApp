﻿using BAExamApp.Dtos.ExamClassrooms;
using BAExamApp.Dtos.Exams;
using System.Globalization;

namespace BAExamApp.Business.Services;

public class ExamService : IExamService
{
    private readonly IExamRepository _examRepository;
    private readonly IClassroomRepository _classroomRepository;
    private readonly IExamRuleRepository _examRuleRepository;
    private IMapper _mapper;
    private readonly IExamClassroomsRepository _examClassroomsRepository;

    public ExamService(IExamRepository examRepository, IClassroomRepository classroomRepository, IExamRuleRepository examRuleRepository, IMapper mapper, IExamClassroomsRepository examClassroomsRepository)
    {
        _examRepository = examRepository;
        _classroomRepository = classroomRepository;
        _examRuleRepository = examRuleRepository;
        _mapper = mapper;
        _examClassroomsRepository = examClassroomsRepository;
    }

    public async Task<IDataResult<ExamDto>> GetByIdAsync(Guid id)
    {
        var exam = await _examRepository.GetByIdAsync(id);

        if (exam != null)
        {
            return new SuccessDataResult<ExamDto>(_mapper.Map<ExamDto>(exam), Messages.ExamFoundSuccess);
        }

        return new ErrorDataResult<ExamDto>(Messages.ExamNotFound);
    }

    public async Task<IDataResult<List<ExamListDto>>> GetByIdentityIdAsync(string id)
    {
        var exams = await _examRepository.GetAllAsync(e => e.CreatedBy == id);
        var examListDtos = _mapper.Map<List<ExamListDto>>(exams);

        return new SuccessDataResult<List<ExamListDto>>(_mapper.Map<List<ExamListDto>>(examListDtos), Messages.ExamFoundSuccess);
    }

    public async Task<IDataResult<List<ExamListDto>>> GetAllAsync()
    {
        var exams = await _examRepository.GetAllAsync();

        return new SuccessDataResult<List<ExamListDto>>(_mapper.Map<List<ExamListDto>>(exams), Messages.ListedSuccess);
    }

    public async Task<IDataResult<ExamDetailDto>> GetDetailsByIdAsync(Guid id)
    {
        var exam = await _examRepository.GetByIdAsync(id);

        if (exam is null)
        {
            return new ErrorDataResult<ExamDetailDto>(Messages.ExamNotFound);
        }

        return new SuccessDataResult<ExamDetailDto>(_mapper.Map<ExamDetailDto>(exam), Messages.FoundSuccess);
    }

    public async Task<IDataResult<ExamDto>> AddAsync(ExamCreateDto examCreateDto)
    {
        //var hasExam = await _examRepository.AnyAsync(x => x.ClassroomId == examCreateDto.ClassroomId && x.ExamRuleId == examCreateDto.ExamRuleId);
        //if (hasExam)
        //{
        //    return new ErrorDataResult<ExamDto>(Messages.AddFailAlreadyExists);
        //}

        var classroomNameList = examCreateDto.ExamClassroomsIds.Select(async x => (await _classroomRepository.GetByIdAsync(x.ClassroomId))).Select(x => x.Result!.Name).ToList();
        var classroomNames = string.Join(" - ", classroomNameList);
        var examRuleName = (await _examRuleRepository.GetByIdAsync(examCreateDto.ExamRuleId))!.Name;
        examCreateDto.Name = $"{classroomNames} - {examRuleName}";
        var exam = _mapper.Map<Exam>(examCreateDto);

        await _examRepository.AddAsync(exam);
        await _examRepository.SaveChangesAsync();

        return new SuccessDataResult<ExamDto>(_mapper.Map<ExamDto>(exam), Messages.AddSuccess);
    }

    public async Task<IResult> DeleteAsync(Guid id)
    {
        var exam = await _examRepository.GetByIdAsync(id);

        if (exam is null)
        {
            return new ErrorResult(Messages.ExamNotFound);
        }

        await _examRepository.DeleteAsync(exam);
        await _examRepository.SaveChangesAsync();

        return new SuccessResult(Messages.DeleteSuccess);
    }

    public async Task<IDataResult<List<ExamListDto>>> GetExamsByStatusAsync(string status)
    {
        var exams = await _examRepository.GetAllAsync();
        var currentDate = DateTime.Now;

        if (status == "past")
        {
            var filteredExams = exams.Where(e => (e.ExamDateTime + e.ExamDuration) < currentDate).ToList();
            return new SuccessDataResult<List<ExamListDto>>(_mapper.Map<List<ExamListDto>>(filteredExams), Messages.ListedSuccess);
        }
        else if (status == "ongoing")
        {
            var filteredExams = exams.Where(e => e.ExamDateTime <= currentDate && currentDate <= (e.ExamDateTime + e.ExamDuration)).ToList();
            return new SuccessDataResult<List<ExamListDto>>(_mapper.Map<List<ExamListDto>>(filteredExams), Messages.ListedSuccess);
        }
        else if (status == "upcoming")
        {
            var filteredExams = exams.Where(e => e.ExamDateTime > currentDate).ToList();
            return new SuccessDataResult<List<ExamListDto>>(_mapper.Map<List<ExamListDto>>(filteredExams), Messages.ListedSuccess);
        }
        else
        {
            return new SuccessDataResult<List<ExamListDto>>(_mapper.Map<List<ExamListDto>>(exams), Messages.ListedSuccess);
        }


    }
    /// <summary>
    /// Kullanıcının yaptığı filtreleme seçimlerine göre sınav listesinin filtreli halini döndürüyor.
    /// </summary>
    /// <param name="selectedClassroomId">Seçilen Sınıf idsini getiriyor.</param>
    /// <param name="selectedRulenameId">Seçilen Kural adının idsini getiriyor.</param>
    /// <param name="datetimePickerStart">Filtrelemek için seçilen başlangıç tarihini getiriyor.</param>
    /// <param name="datetimePickerEnd">Filtrelemek için seçilen bitiş tarihini getiriyor.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public async Task<IDataResult<List<ExamListDto>>> GetExamsByFiltered(string selectedClassroomId, string selectedRulenameId, string datetimePickerStart, string datetimePickerEnd, bool isActiveExams)
    {
        var exams = await _examRepository.GetAllAsync();
        var clasExams = await _examClassroomsRepository.GetAllAsync();
        var commonExamIds = clasExams.Select(x => x.ExamId).Intersect(exams.Select(x => x.Id));

        List<ExamListDto> examDtoList = new List<ExamListDto>();
        List<(string?, decimal?, string?)> tooltipStudentList = new List<(string?, decimal?, string?)>();
        foreach (var item in commonExamIds)
        {
            var examsForItem = clasExams.Where(x => x.ExamId == item).ToList();
            var studentData = examsForItem
        .SelectMany(x => x.Exam.StudentExams.Select(se => new
        {
            StudentName = se.Student.FullName,
            StudentScore = se.Score,
            ClassroomName = se.Student.StudentClassrooms.Select(sc => sc.Classroom.Name).FirstOrDefault(),
        })).Distinct()
        .ToList();
            var examDto = new ExamListDto
            {
                Id = item,
                ClassroomNames = examsForItem.Select(x => x.Classroom.Name).ToList(),
                ClassroomId = examsForItem.First().Classroom.Id,
                ExamDateTime = examsForItem.FirstOrDefault().Exam.ExamDateTime,
                ExamDuration = examsForItem.FirstOrDefault().Exam.ExamDuration,
                ExamRuleName = examsForItem.FirstOrDefault().Exam.ExamRule.Name,
                RuleId = examsForItem.FirstOrDefault().Exam.ExamRule.Id,
                Name = examsForItem.FirstOrDefault().Exam.Name,
                StudentExamScore = examsForItem.SelectMany(x => x.Exam.StudentExams.Select(y => y.Score).ToList()).ToList(),
                StudentName = examsForItem.SelectMany(x => x.Exam.StudentExams.Select(x => x.Student.FullName).ToList()).Distinct().ToList(),
                tooltipStudentList = studentData.Select(sd => (sd.StudentName, sd.StudentScore, sd.ClassroomName)).Distinct().ToList(),

            };

            examDtoList.Add(examDto);
        }

        if (isActiveExams)
        {
            return new SuccessDataResult<List<ExamListDto>>(_mapper.Map<List<ExamListDto>>(examDtoList), Messages.ListedSuccess);
        }
        else
        {
            var filteredExams = examDtoList.Where(x =>
            (string.IsNullOrEmpty(selectedClassroomId) || (x.ClassroomId.ToString() == selectedClassroomId)) &&
            (string.IsNullOrEmpty(selectedRulenameId) || x.RuleId.ToString() == selectedRulenameId) &&
            (string.IsNullOrEmpty(datetimePickerStart) || x.ExamDateTime > ParseDateTimeWithT(datetimePickerStart)) &&
            (string.IsNullOrEmpty(datetimePickerEnd) || (x.ExamDateTime + x.ExamDuration) < ParseDateTimeWithT(datetimePickerEnd)));

            return new SuccessDataResult<List<ExamListDto>>(_mapper.Map<List<ExamListDto>>(filteredExams), Messages.ListedSuccess);
        }
        DateTime ParseDateTimeWithT(string dateTimeString)
        {
            if (DateTime.TryParseExact(dateTimeString, "yyyy-MM-ddTHH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDateTime))
            {
                return parsedDateTime;
            }
            else
            {
                throw new ArgumentException("Geçersiz tarih/saat formatı");
            }
        }
    }

    /// <summary>
    /// Seçilen sınıfın tüm sınavlarını döndürür
    /// </summary>
    /// <param name="classroomId">Seçilen sınıfın sınavlarını döndürmesi için o sınıfın id'sine ihtiyaç vardır.</param>
    /// <returns></returns>
    public async Task<IDataResult<List<ExamListDto>>> GetExamsByClassIdAsync(Guid classroomId)
    {
        var classExams = await _examRepository.GetAllAsync(x => x.ExamClassrooms.Any(ec => ec.ClassroomId == classroomId));
        var examListDto = _mapper.Map<List<ExamListDto>>(classExams);

        foreach (var examDto in examListDto)
        {
            var studentCount = examDto.StudentName.Count();
            examDto.StudentCount= studentCount;

            var examCount = examDto.StudentExamScore.Where(x=>x.HasValue).Count();
            examDto.StudentExamScoreCount=examCount;

            var examStudentScores = examDto.StudentExamScore;
            var examAverage = examStudentScores.Any() ? examStudentScores.Average() : 0;
            examDto.ClassGradeAverage = examAverage;
        }

        return new SuccessDataResult<List<ExamListDto>>(examListDto, Messages.ListedSuccess);
    }


}
