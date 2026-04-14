import os

def main():
    """
    Finds all .cs files ONLY within the script's own directory and its subdirectories.
    The results are combined into a single text file.
    """
    try:
        # --- KEY CHANGE ---
        # The search root is now the directory where the script itself is located.
        # The search will not go outside of this folder.
        project_folder = os.path.dirname(os.path.abspath(__file__))
        
        # The output file is saved in the same project folder.
        output_filename = "birlestirilmis_proje_scriptleri.txt"
        output_file_path = os.path.join(project_folder, output_filename)

        print("🔎 Project-Only Search Mode Activated")
        print(f"Project Folder (Search Root): {project_folder}")
        print("-" * 60)
        
        found_any_files = False

        with open(output_file_path, "w", encoding="utf-8") as outfile:
            # Walk through the project folder
            for dirpath, _, filenames in os.walk(project_folder):
                # To prevent the script from reading its own output file in a future run, we skip it.
                if output_filename in filenames and dirpath == project_folder:
                    filenames.remove(output_filename)

                for filename in filenames:
                    # We also skip the python script itself
                    if filename.endswith(".cs"):
                        found_any_files = True
                        
                        full_path = os.path.join(dirpath, filename)
                        
                        # Get the path relative to the project folder for a clean header
                        relative_path = os.path.relpath(full_path, project_folder)

                        print(f"-> Found: {relative_path}")
                        
                        # Add a C#-style comment header
                        outfile.write(f"//--- File Source: {relative_path} ---\n\n")
                        
                        try:
                            with open(full_path, "r", encoding="utf-8") as infile:
                                outfile.write(infile.read())
                            outfile.write("\n\n")
                        except Exception as e:
                            error_message = f"//--- ERROR reading file {relative_path}: {e} ---\n\n"
                            outfile.write(error_message)
                            print(f"   [!] Could not read file: {relative_path}. Error: {e}")

        # --- Final Status Message ---
        if found_any_files:
            print(f"\n✅ Success! All project .cs files have been combined into '{output_filename}'.")
        else:
            print(f"\n⏹️ No .cs files were found in the project folder: {project_folder}")

    except Exception as e:
        print(f"\n❌ An unexpected error occurred: {e}")

    finally:
        input("\nPress Enter to exit...")

if __name__ == "__main__":
    main()