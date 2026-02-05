export class SkillMatcherDTO {
  shiftTypeId = 0;
  shiftTypeName = '';
  skillId = 0;
  skillName = '';
  skillLevel = 0;
  skillRating = 0;
  missing = false;
  employeeSkillLevel = 0;
  employeeSkillRating = 0;
  skillLevelUnreached = false;
  dateTo?: Date;
  dateToPassed = false;
  note = '';
  ok = false;
  visible = false;
  selected = false;
}
