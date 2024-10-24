using BUS;
using DAL.Entity;
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;

namespace Lab05
{
    public partial class Form1 : Form
    {
        // Sử dụng các dịch vụ để quản lý sinh viên và khoa
        private FacultyService facultyService = new FacultyService();
        private StudentService stService = new StudentService();
        private string selectedAvatarPath = null; // Lưu đường dẫn ảnh đã chọn

        public Form1()
        {
            InitializeComponent();
            LoadFaculties(); // Load danh sách khoa vào ComboBox
            LoadStudents();  // Hiển thị tất cả sinh viên khi khởi động
            dataGridView1.CellClick += dataGridView1_CellClick; // Sự kiện chọn dòng trong DataGridView
        }

        // Tải danh sách sinh viên từ database lên DataGridView
        public void LoadStudents()
        {
            bool withoutMajor = cbDK.Checked; // Kiểm tra trạng thái của CheckBox

            // Lấy danh sách sinh viên từ StudentService
            var students = stService.GetStudentList(withoutMajor).Select(s => new
            {
                MSSV = s.StudentID,
                HoTen = s.FullName,
                Khoa = s.Faculty.FacultyName,
                DTB = s.AverageScore,
                ChuyenNganh = s.Major?.Name ?? "" // Nếu chưa có chuyên ngành thì để trống
            }).ToList();

            dataGridView1.DataSource = students; // Hiển thị danh sách sinh viên
        }


        // Load danh sách khoa vào ComboBox
        private void LoadFaculties()
        {
            var faculties = facultyService.GetFacultyList();
            cmbKhoa.DataSource = faculties;
            cmbKhoa.DisplayMember = "FacultyName";
            cmbKhoa.ValueMember = "FacultyID";
        }

        // Sự kiện khi CheckBox thay đổi trạng thái
        private void cbDK_CheckedChanged(object sender, EventArgs e)
        {
            LoadStudents(); // Cập nhật danh sách sinh viên theo trạng thái CheckBox
        }

        // Thêm hoặc cập nhật sinh viên
        private void btnThem_Click(object sender, EventArgs e)
        {
            Student student = stService.FindStudentByID(txtMSSV.Text);
            bool isNew = student == null;

            if (isNew)
                student = new Student();

            student.StudentID = txtMSSV.Text;
            student.FullName = txtHoTen.Text;
            student.AverageScore = float.Parse(txtDTB.Text);
            student.FacultyID = (int)cmbKhoa.SelectedValue;

            // Lưu tên ảnh vào CSDL nếu có ảnh đại diện
            if (!string.IsNullOrEmpty(selectedAvatarPath))
            {
                string extension = Path.GetExtension(selectedAvatarPath);
                student.Avatar = $"{student.StudentID}{extension}";
                SaveAvatar(selectedAvatarPath, student.Avatar); // Lưu ảnh vào thư mục Images
            }

            stService.InsertOrUpdateStudent(student); // Thêm hoặc cập nhật sinh viên
            LoadStudents(); // Cập nhật danh sách sau khi thêm/cập nhật
        }

        // Xóa sinh viên
        private void btnXoa_Click(object sender, EventArgs e)
        {
            string studentID = txtMSSV.Text;
            var student = stService.FindStudentByID(studentID);

            if (student != null)
            {
                stService.DeleteStudent(studentID);
                MessageBox.Show("Xóa sinh viên thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadStudents(); // Cập nhật danh sách sau khi xóa
            }
            else
            {
                MessageBox.Show("Không tìm thấy sinh viên!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Sự kiện khi bấm vào một hàng trong DataGridView
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];

                // Hiển thị dữ liệu vào các TextBox, ComboBox
                txtMSSV.Text = row.Cells["MSSV"].Value.ToString();
                txtHoTen.Text = row.Cells["HoTen"].Value.ToString();
                txtDTB.Text = row.Cells["DTB"].Value.ToString();
                cmbKhoa.SelectedIndex = cmbKhoa.FindStringExact(row.Cells["Khoa"].Value.ToString());

                // Hiển thị ảnh đại diện nếu có
                string avatarFileName = row.Cells["MSSV"].Value.ToString() + Path.GetExtension(selectedAvatarPath);
                string avatarPath = Path.Combine(@"C:\Users\admin1\source\repos\Lab05\Images", avatarFileName);

                if (File.Exists(avatarPath))
                    pictureBox1.Image = Image.FromFile(avatarPath);
                else
                    pictureBox1.Image = null;
            }
        }

        // Sự kiện chọn ảnh đại diện
        private void btnChon_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    selectedAvatarPath = openFileDialog.FileName; // Lưu đường dẫn ảnh đã chọn
                    pictureBox1.Image = Image.FromFile(selectedAvatarPath); // Hiển thị ảnh đã chọn
                }
            }
        }

        // Lưu ảnh vào thư mục Images
        private void SaveAvatar(string sourcePath, string fileName)
        {
            // Xác định đường dẫn thư mục "Images" tuyệt đối
            string folderPath = @"C:\Users\admin1\source\repos\Lab05\Images";

            // Kiểm tra và tạo thư mục nếu chưa tồn tại
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath); // Tạo thư mục nếu chưa có

            // Tạo đường dẫn đầy đủ để lưu ảnh
            string savePath = Path.Combine(folderPath, fileName);

            // Lưu ảnh vào đường dẫn chỉ định
            File.Copy(sourcePath, savePath, true);
        }

        // Mở form đăng ký chuyên ngành
        private void đăngKýChuyênNgànhToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmRegister registerForm = new frmRegister();
            registerForm.Owner = this;  // Thiết lập Owner để form con có thể gọi phương thức của form chính
            registerForm.ShowDialog();  // Hiển thị form đăng ký chuyên ngành
        }
    }
}
