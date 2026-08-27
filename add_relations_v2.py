#!/usr/bin/env python3
"""Add RELATIONS sections to test cases (optimized line-by-line approach)."""
import json

def load_mappings(mapping_file):
    """Load TC to RQ mappings."""
    with open(mapping_file, 'r') as f:
        return json.load(f)

def add_relations_to_file(filepath, mappings, output_filepath):
    """Add RELATIONS to test cases."""
    tc_to_rq = {item['test_case_uid']: item['requirement_uids'] for item in mappings}
    
    with open(filepath, 'r') as f:
        lines = f.readlines()
    
    output_lines = []
    i = 0
    current_test_case_uid = None
    current_test_case_line = None
    
    while i < len(lines):
        line = lines[i]
        
        # Check if this is a TEST_CASE start
        if line.strip() == '[TEST_CASE]':
            current_test_case_line = i
        
        # Check if this is UID line in a TEST_CASE
        elif line.startswith('UID:') and current_test_case_line is not None:
            current_test_case_uid = line.split(':', 1)[1].strip()
        
        # Check if we're at the end of a TEST_CASE (next [TEST_CASE], [[/SECTION]], or end of file)
        elif (line.strip().startswith('[TEST_CASE]') or 
              line.strip().startswith('[[/SECTION]]') or 
              (i == len(lines) - 1)):
            
            # If we have a current test case with no RELATIONS, insert one before this line
            if (current_test_case_uid and 
                current_test_case_uid in tc_to_rq and
                current_test_case_line is not None):
                
                # Check if RELATIONS already exists by looking back
                has_relations = False
                for j in range(current_test_case_line, i):
                    if 'RELATIONS:' in lines[j]:
                        has_relations = True
                        break
                
                if not has_relations:
                    # Insert RELATIONS before current line
                    requirement_uids = tc_to_rq[current_test_case_uid]
                    output_lines.append('RELATIONS:\n')
                    for rq_uid in requirement_uids:
                        output_lines.append(f'- TYPE: Parent\n')
                        output_lines.append(f'  VALUE: {rq_uid}\n')
                        output_lines.append(f'  ROLE: Verifies\n')
            
            # Reset for next test case
            current_test_case_uid = None
            current_test_case_line = None
            output_lines.append(line)
        else:
            output_lines.append(line)
        
        i += 1
    
    # Write output
    with open(output_filepath, 'w') as f:
        f.writelines(output_lines)

def main():
    mapping_file = '/home/runner/work/gentlebeam-sk/gentlebeam-sk/tc_rq_mappings.json'
    mappings = load_mappings(mapping_file)
    
    # Process external
    external_file = '/home/runner/work/gentlebeam-sk/gentlebeam-sk/cnc/voxelray-gentlebeamcnc/docs/external/main_test_protocol.sdoc'
    print(f"Processing external...")
    add_relations_to_file(external_file, mappings['external'], external_file)
    print(f"✓ Updated external test protocol")
    
    # Process internal
    internal_file = '/home/runner/work/gentlebeam-sk/gentlebeam-sk/cnc/voxelray-gentlebeamcnc/docs/internal/main_test_protocol.sdoc'
    print(f"Processing internal...")
    add_relations_to_file(internal_file, mappings['internal'], internal_file)
    print(f"✓ Updated internal test protocol")

if __name__ == '__main__':
    main()
