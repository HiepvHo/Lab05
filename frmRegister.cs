using System;
using System.Linq;
using System.Windows.Forms;
using BUS; // Sử dụng các Service từ tầng BUS
using DAL.Entity; // Truy cập các Entity từ tầng DAL

namespace Lab05
{
    public partial class frmRegister : Form
    {
        private StudentService studentService = new StudentService();  // Sử dụng StudentService để thao tác với sinh viên
        private MajorService majorService = new MajorService();        // Sử dụng MajorService để thao tác với chuyên ngành
        private FacultyService facultyService = new FacultyService();  // Sử dụng FacultyService để thao tác với khoa
        private string selectedMajorID = null;  // Biến lưu MajorID đã chọn

        public frmRegister()
        {
            InitializeComponent();
            LoadFaculties(); // Load danh sách khoa khi khởi động form
        }

        // Load danh sách khoa vào ComboBox Khoa
        private void LoadFaculties()
        {
            var faculties = facultyService.GetFacultyList();
            cmbKhoa.DataSource = faculties;
            cmbKhoa.DisplayMember = "FacultyName";  // Hiển thị tên khoa
            cmbKhoa.ValueMember = "FacultyID";      // Lưu giá trị FacultyID
        }

        // Khi người dùng chọn Khoa, load danh sách chuyên ngành tương ứng
        private void cmbKhoa_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Kiểm tra nếu SelectedValue không phải là null và có thể chuyển đổi sang kiểu int
            if (cmbKhoa.SelectedValue != null)
            {
                int selectedFacultyID;
                if (int.TryParse(cmbKhoa.SelectedValue.ToString(), out selectedFacultyID))
                {
                    // Lấy danh sách chuyên ngành theo Khoa đã chọn
                    var majors = majorService.GetMajorsByFaculty(selectedFacultyID);
                    cmbnganh.DataSource = majors;
                    cmbnganh.DisplayMember = "Name";      // Hiển thị tên chuyên ngành
                    cmbnganh.ValueMember = "MajorID";     // Lưu giá trị MajorID

                    // Hiển thị sinh viên chưa có chuyên ngành thuộc Khoa đã chọn
                    LoadStudentsWithoutMajor(selectedFacultyID);
                }
            }
        }

        // Tải danh sách sinh viên chưa có chuyên ngành của khoa được chọn
        private void LoadStudentsWithoutMajor(int facultyID)
        {
            // Lấy dữ liệu sinh viên chưa có chuyên ngành
            var students = studentService.GetStudentsWithoutMajorByFaculty(facultyID)
                .Select(s => new
                {
                    MSSV = s.StudentID,
                    HoTen = s.FullName,
                    Khoa = s.Faculty.FacultyName,
                    DTB = s.AverageScore
                }).ToList();

            // Reset lại DataGridView
            dataGridView1.DataSource = null;
            dataGridView1.Columns.Clear();

            // Thêm cột CheckBox
            DataGridViewCheckBoxColumn checkBoxColumn = new DataGridViewCheckBoxColumn();
            checkBoxColumn.HeaderText = "Chọn";
            checkBoxColumn.Name = "Chon";
            checkBoxColumn.ReadOnly = false; // Cho phép chọn
            dataGridView1.Columns.Add(checkBoxColumn);

            // Gán dữ liệu sinh viên vào DataGridView
            dataGridView1.DataSource = students;

            // Thiết lập các cột dữ liệu khác là ReadOnly
            dataGridView1.Columns["MSSV"].ReadOnly = true;
            dataGridView1.Columns["HoTen"].ReadOnly = true;
            dataGridView1.Columns["Khoa"].ReadOnly = true;
            dataGridView1.Columns["DTB"].ReadOnly = true;
        }

        // Xử lý sự kiện click "Register" để đăng ký chuyên ngành cho sinh viên được chọn
        private void btndk_Click(object sender, EventArgs e)
        {
            if (cmbnganh.SelectedValue != null)
            {
                int selectedMajorID = (int)cmbnganh.SelectedValue; // Lấy MajorID từ ComboBox chuyên ngành

                // Duyệt qua các dòng trong DataGridView và kiểm tra sinh viên nào được chọn
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    bool isSelected = Convert.ToBoolean(row.Cells["Chon"].Value); // Kiểm tra nếu sinh viên được chọn

                    if (isSelected)
                    {
                        string studentID = row.Cells["MSSV"].Value.ToString();
                        var student = studentService.FindStudentByID(studentID);

                        // Gán chuyên ngành đã chọn cho sinh viên
                        if (student != null)
                        {
                            student.MajorID = selectedMajorID;
                            studentService.InsertOrUpdateStudent(student); // Cập nhật sinh viên
                        }
                    }
                }

                MessageBox.Show("Đăng ký chuyên ngành thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Cập nhật form chính ngay khi nhấn OK
                var mainForm = (Form1)this.Owner;
                mainForm.LoadStudents();  // Cập nhật danh sách sinh viên trong form chính

                this.Close(); // Đóng form đăng ký sau khi hoàn tất
            }
        }

        // Khi chọn chuyên ngành (nếu có thay đổi logic theo chuyên ngành thì viết tại đây)
        private void cmbnganh_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Chức năng bổ sung nếu cần
        }

        // Khi tắt form, cập nhật danh sách sinh viên trên form chính

        private void frmRegister_FormClosed_1(object sender, FormClosedEventArgs e)
        {
            var mainForm = (Form1)this.Owner;
            mainForm.LoadStudents(); // Cập nhật danh sách sinh viên trong form chính
        }
    }
}
