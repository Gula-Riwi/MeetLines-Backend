import os

root_dir = '.'
target = 'MeetLines'
replacement = 'MeetLines'

print(f"Starting replacement of '{target}' with '{replacement}' in '{os.path.abspath(root_dir)}'...")

for subdir, dirs, files in os.walk(root_dir):
    if '.git' in dirs:
        dirs.remove('.git')
    
    for file in files:
        if file == 'rename_content.py':
            continue
            
        file_path = os.path.join(subdir, file)
        try:
            # Try reading as utf-8
            try:
                with open(file_path, 'r', encoding='utf-8') as f:
                    content = f.read()
            except UnicodeDecodeError:
                # Skip binary files or non-utf-8
                print(f"Skipping binary/non-utf-8 file: {file_path}")
                continue
            
            if target in content:
                new_content = content.replace(target, replacement)
                with open(file_path, 'w', encoding='utf-8') as f:
                    f.write(new_content)
                print(f"Updated: {file_path}")
        except Exception as e:
            print(f"Error processing {file_path}: {e}")

print("Replacement complete.")
