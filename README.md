# **Lecturer Claim Submission System**

## **Overview**

The Lecturer Claim Submission System is a web-based application developed using ASP.NET Core MVC. It allows lecturers to submit their monthly claims (hours worked, hourly rate, supporting documents) and provides program coordinators and academic managers the ability to review, approve, or reject these claims.

In this update for Part 3, the focus has been on enhancing the user experience (UX) and design, ensuring that the process is both efficient and intuitive for all users. This README explains the improvements made to the application’s design, usability, and overall flow, addressing feedback from Part 2.

## **Key Features**

### **1. Simple and Intuitive Claim Submission Form**

The claim submission form has been designed with simplicity and clarity in mind. Here’s how it has been improved:

* **Clear Input Fields**: The form now clearly guides lecturers through the submission process with well-labeled input fields, such as "Hours Worked," "Hourly Rate," and "Additional Notes."
* **File Upload**: Lecturers can easily upload supporting documents such as PDFs, Word documents, and Excel files. The form validates file types and limits file size to ensure compatibility with the system.
* **Prominent Submit Button**: The Submit Claim button is designed to stand out, making it easy for lecturers to finalize their claim submission with a single click.

### **2. Improved Design for Easier Navigation**

The user interface has been enhanced to make it more user-friendly and visually appealing:

* **Card-style Containers**: Each section of the application (e.g., claim submission, claim review) is neatly organized within a card-style container to keep content visually separated and easy to follow.
* **Clean and Modern Design**: A minimalistic color scheme and modern UI elements (rounded buttons, input fields, etc.) create a clean and professional look. This makes navigating through the application seamless and visually appealing.

### **3. Efficient Claim Review Dashboard for Managers**

The coordinator and manager dashboard has been designed to provide a clear overview of all claims, making it easier to verify, approve, or reject claims:

* **Inline Document Viewer**: The system allows managers to preview supporting documents without the need to download them, saving time and simplifying the process.
* **Approval and Rejection Actions**: Each claim in the table has approve and reject buttons. Managers can add remarks when approving or rejecting claims, allowing for easy tracking and communication.

### **4. Real-Time Status Updates**

The application now includes real-time status tracking of each claim:

* **Status Labels**: Claims are assigned a status of Pending, Approved, or Rejected. The status is updated in real-time whenever a manager or coordinator makes a decision.
* **Visual Feedback**: Upon updating a claim’s status, the claim’s status label changes color (green for approved, red for rejected, orange for pending) for better visual identification.

### **5. Error Handling and File Upload Validation**

To ensure a smooth experience for users:

* **File Size and Type Validation**: Uploaded files are validated for size (5MB limit) and file type (PDF, DOCX, XLSX). This prevents incompatible files from being uploaded.
* **Error Messages**: Clear and meaningful error messages are displayed to users when they try to upload an invalid file or submit incomplete information. This helps users fix issues before submitting the form.

## **How the Design Has Been Improved**

### **1. Consistency Across Pages**

One of the primary improvements is the consistent design throughout the application. From the claim submission form to the claim review dashboard, every page follows the same layout structure, color scheme, and typography. This ensures a unified user experience and helps users feel comfortable navigating through the application.

### **2. Streamlined User Flow**

The claim submission process is now much easier:

* Lecturers can submit their claims quickly by simply filling in the form and uploading supporting documents. The submit button is easy to locate, and all required fields are clearly labeled.
* Managers and Coordinators have a streamlined review process. Claims are displayed in a clean, sortable table with the option to approve or reject each claim directly from the dashboard.
* The feedback loop is also clear — lecturers receive confirmation upon submitting a claim, and coordinators/managers can leave remarks when reviewing claims.

### **3. Improved Visual Design**

* **Card-style design** for claims and claim review provides a modern, clean look.
* **Buttons and forms** have rounded corners and soft shadows, making them feel more interactive and modern.
* **Status indicators** use color to show whether a claim is pending, approved, or rejected, allowing users to quickly understand the state of each claim.

## **How the Application Works**

### **For Lecturers**

* **Submit Claims**: Lecturers can submit their claims by providing their hours worked, hourly rate, and supporting documents.
* **Track Status**: After submission, lecturers can track the status of their claims in real time as they move through the approval process.

### **For Coordinators and Managers**

* **Review Claims**: Coordinators and managers can view all submitted claims in an easy-to-read table.
* **Approve or Reject**: They can approve or reject claims directly from the dashboard, leaving optional remarks.
* **Preview Documents**: Managers can preview uploaded documents inline to verify the claim without needing to download files.

## **Conclusion**

With these improvements, the Lecturer Claim Submission System is now easier to use, more intuitive, and more visually appealing. The redesigned user interface enhances the experience for both lecturers and coordinators/managers, making the process of submitting, reviewing, and approving claims much faster and smoother.

By focusing on user-centered design principles, the application is now more efficient, more accessible, and more reliable, ensuring that users can complete their tasks with minimal frustration and maximum ease.

