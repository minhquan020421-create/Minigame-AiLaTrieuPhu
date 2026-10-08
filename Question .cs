using System;

namespace AiLaTrieuPhu
{
    // Ba tầng độ khó của SPEC v2:
    // Dễ: câu 1–3.
    // Trung bình: câu 4–7.
    // Khó: câu 8–10.
    public enum QuestionDifficulty
    {
        Easy,
        Medium,
        Hard
    }

    // Mô tả một câu hỏi.
    // Lớp này chỉ giữ và kiểm tra dữ liệu câu hỏi,
    // không xử lý tiền thưởng hoặc tham chiếu giao diện.
    public class Question
    {
        // Giữ riêng mảng đáp án bên trong đối tượng.
        // Không cung cấp trực tiếp mảng này cho các phần khác.
        private readonly string[] _options;

        // Mã nhận diện câu hỏi.
        // Mã được giữ nguyên khi trộn đáp án hoặc chọn sang ván mới.
        public string Id { get; }

        // Nội dung câu hỏi để hiển thị cho người chơi.
        public string Text { get; }

        // Bốn đáp án theo thứ tự đang sử dụng:
        // vị trí 0 = A, 1 = B, 2 = C, 3 = D.
        // Mỗi lần đọc sẽ nhận một bản sao của mảng,
        // tránh việc bên ngoài sửa dữ liệu của câu hỏi.
        public string[] Options
        {
            get
            {
                return (string[])_options.Clone();
            }
        }

        // Vị trí hiện tại của đáp án đúng trong mảng đáp án.
        // Giá trị hợp lệ luôn nằm trong khoảng 0–3.
        public int CorrectAnswerIndex { get; }

        // Tầng độ khó dùng để phân nhóm trong ngân hàng câu hỏi.
        public QuestionDifficulty Difficulty { get; }

        // Trả về nội dung đáp án đúng theo vị trí hiện tại.
        // Không thay đổi dữ liệu và không sao chép cả mảng đáp án.
        public string GetCorrectAnswerText()
        {
            return _options[CorrectAnswerIndex];
        }

        // Hàm khởi tạo: nhận đủ năm thông tin của một câu hỏi.
        // Dữ liệu phải vượt qua toàn bộ kiểm tra bên dưới
        // trước khi đối tượng được tạo thành công.
        public Question(
            string id,
            string text,
            string[] options,
            int correctAnswerIndex,
            QuestionDifficulty difficulty)
        {
            // Không chấp nhận mã bị thiếu, rỗng
            // hoặc chỉ chứa khoảng trắng.
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException(
                    "Mã câu hỏi không được để trống.",
                    nameof(id));
            }

            // Không chấp nhận nội dung bị thiếu, rỗng
            // hoặc chỉ chứa khoảng trắng.
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentException(
                    "Nội dung câu hỏi không được để trống.",
                    nameof(text));
            }

            // Kiểm tra mảng có tồn tại trước khi đọc độ dài
            // hoặc truy cập các phần tử.
            if (options == null)
            {
                throw new ArgumentNullException(
                    nameof(options),
                    "Danh sách đáp án không được thiếu.");
            }

            // Mỗi câu bắt buộc có đúng bốn lựa chọn.
            if (options.Length != 4)
            {
                throw new ArgumentException(
                    "Mỗi câu hỏi phải có đúng 4 đáp án.",
                    nameof(options));
            }

            // Chỉ số bắt đầu từ 0 nên đáp án thứ tư có chỉ số 3.
            if (correctAnswerIndex < 0 || correctAnswerIndex > 3)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(correctAnswerIndex),
                    "Chỉ số đáp án đúng phải từ 0 đến 3.");
            }

            // Bảo đảm tầng độ khó thuộc ba giá trị được cho phép.
            if (difficulty != QuestionDifficulty.Easy &&
                difficulty != QuestionDifficulty.Medium &&
                difficulty != QuestionDifficulty.Hard)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(difficulty),
                    "Tầng độ khó không hợp lệ.");
            }

            // Tạo mảng mới thuộc riêng đối tượng câu hỏi.
            // Việc bên gọi sửa mảng ban đầu sẽ không ảnh hưởng
            // đến dữ liệu đã lưu trong đối tượng.
            string[] copiedOptions = new string[4];

            // Kiểm tra lần lượt cả bốn lựa chọn.
            for (int i = 0; i < options.Length; i++)
            {
                // Mỗi đáp án phải có nội dung thực sự.
                if (string.IsNullOrWhiteSpace(options[i]))
                {
                    throw new ArgumentException(
                        "Đáp án ở chỉ số " + i +
                        " không được để trống.",
                        nameof(options));
                }

                // Bỏ khoảng trắng đầu và cuối,
                // giữ nguyên thứ tự cũng như nội dung bên trong.
                copiedOptions[i] = options[i].Trim();

                // Đối chiếu đáp án hiện tại với các đáp án trước đó.
                // Không phân biệt chữ hoa và chữ thường khi xét trùng.
                for (int j = 0; j < i; j++)
                {
                    if (string.Equals(
                        copiedOptions[i],
                        copiedOptions[j],
                        StringComparison.OrdinalIgnoreCase))
                    {
                        throw new ArgumentException(
                            "Các đáp án trong cùng một câu " +
                            "không được trùng nhau.",
                            nameof(options));
                    }
                }
            }

            // Chỉ lưu thông tin sau khi dữ liệu đã hợp lệ.
            // Việc bỏ khoảng trắng không làm thay đổi vị trí đáp án.
            Id = id.Trim();
            Text = text.Trim();
            _options = copiedOptions;
            CorrectAnswerIndex = correctAnswerIndex;
            Difficulty = difficulty;
        }
    }
}