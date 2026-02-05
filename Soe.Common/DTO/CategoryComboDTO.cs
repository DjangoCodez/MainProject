namespace SoftOne.Soe.Common.DTO
{
    public class CategoryComboDTO
    {
        public int? Category1Id { get; set; }
        public string Category1Code { get; set; }
        public string Category1Name { get; set; }

        public int? Category2Id { get; set; }
        public string Category2Code { get; set; }
        public string Category2Name { get; set; }

        public int? Category3Id { get; set; }
        public string Category3Code { get; set; }
        public string Category3Name { get; set; }

        public int? Category4Id { get; set; }
        public string Category4Code { get; set; }
        public string Category4Name { get; set; }

        public int? Category5Id { get; set; }
        public string Category5Code { get; set; }
        public string Category5Name { get; set; }

        public int? Category6Id { get; set; }
        public string Category6Code { get; set; }
        public string Category6Name { get; set; }

        public int? Category7Id { get; set; }
        public string Category7Code { get; set; }
        public string Category7Name { get; set; }

        public int? Category8Id { get; set; }
        public string Category8Code { get; set; }
        public string Category8Name { get; set; }

        public int? Category9Id { get; set; }
        public string Category9Code { get; set; }
        public string Category9Name { get; set; }

        public int? Category10Id { get; set; }
        public string Category10Code { get; set; }
        public string Category10Name { get; set; }

        public void SetCategoryValues(int position, int categoryId, string categoryCode, string categoryName)
        {
            switch (position)
            {
                case 1:
                    this.Category1Id = categoryId;
                    this.Category1Code = categoryCode;
                    this.Category1Name = categoryName;
                    break;
                case 2:
                    this.Category2Id = categoryId;
                    this.Category2Code = categoryCode;
                    this.Category2Name = categoryName;
                    break;
                case 3:
                    this.Category3Id = categoryId;
                    this.Category3Code = categoryCode;
                    this.Category3Name = categoryName;
                    break;
                case 4:
                    this.Category4Id = categoryId;
                    this.Category4Code = categoryCode;
                    this.Category4Name = categoryName;
                    break;
                case 5:
                    this.Category5Id = categoryId;
                    this.Category5Code = categoryCode;
                    this.Category5Name = categoryName;
                    break;
                case 6:
                    this.Category6Id = categoryId;
                    this.Category6Code = categoryCode;
                    this.Category6Name = categoryName;
                    break;
                case 7:
                    this.Category7Id = categoryId;
                    this.Category7Code = categoryCode;
                    this.Category7Name = categoryName;
                    break;
                case 8:
                    this.Category8Id = categoryId;
                    this.Category8Code = categoryCode;
                    this.Category8Name = categoryName;
                    break;
                case 9:
                    this.Category9Id = categoryId;
                    this.Category9Code = categoryCode;
                    this.Category9Name = categoryName;
                    break;
                case 10:
                    this.Category10Id = categoryId;
                    this.Category10Code = categoryCode;
                    this.Category10Name = categoryName;
                    break;
            }
        }
    }
}
